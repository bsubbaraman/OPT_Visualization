using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenPTrack
{
    /// <summary>
    /// A TfListener to receive tf information from ros through rossharp.
	/// It does not include a full tf graph representation, so it is only capable of
    /// getting transform published explicitly, i.e.published explicitly in a tf message.
    /// It can not combine different messages toghether to deduce transforms
    /// </summary>
    /// <author>
    /// Carlo Rizzardo
    /// </author>
	public class TfListener : MonoBehaviour
	{

		///<summary>
		///Compares two TransformStampedMsg based on their timestamps
		///</summary>
		public class ByTime : IComparer<TransformStampedMsg>
		{
			public int Compare(TransformStampedMsg x, TransformStampedMsg y)
			{
				return (int)(x.header.getTimeMillis()-y.header.getTimeMillis());
			}
		}
        [System.Serializable()]
        public class TransformException : System.Exception
        {
            public TransformException() : base() { }
            public TransformException(string message) : base(message) { }
            public TransformException(string message, System.Exception inner) : base(message, inner) { }

            // A constructor is needed for serialization when an
            // exception propagates from a remoting server to the client. 
            protected TransformException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        [System.Serializable()]
        public class LookupException : TransformException
        {
            public LookupException() : base() { }
            public LookupException(string message) : base(message) { }
            public LookupException(string message, System.Exception inner) : base(message, inner) { }

            // A constructor is needed for serialization when an
            // exception propagates from a remoting server to the client. 
            protected LookupException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        [System.Serializable()]
        public class ExtrapolationException : TransformException
        {
            public ExtrapolationException() : base() { }
            public ExtrapolationException(string message) : base(message) { }
            public ExtrapolationException(string message, System.Exception inner) : base(message, inner) { }

            // A constructor is needed for serialization when an
            // exception propagates from a remoting server to the client. 
            protected ExtrapolationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        ///<summary>
		///Class that represent a couple of frames, without order.
		///Used as a key for retreiving transforms
		///</summary>
        private class FrameCouple
		{
			public string frame1;
			public string frame2;

			public FrameCouple(string frame1, string frame2)
			{
				this.frame1=frame1;
				this.frame2=frame2;
			}

			public override string ToString()
			{
				return "["+frame1+", "+frame2+"]";
			}

            ///<summary>
            ///Two of these objects are equal if they contain the same two frames, regardless
            ///of their order
            ///</summary>
            public override bool Equals(object obj)
			{				
				if (obj == null || GetType() != obj.GetType())
					return false;
				
				FrameCouple other = (FrameCouple)obj;
				bool areEqual = (frame1.Equals(other.frame1) && frame2.Equals(other.frame2)) ||
					   (frame1.Equals(other.frame2) && frame1.Equals(other.frame2));
				
				return areEqual;
			}

            ///<summary>
            /// Needed for the dictionary to work properly. Will return the same value for two objects
            /// if they are equal as per Equals(object)
            ///</summary>
            public override int GetHashCode()
			{
				return frame1.GetHashCode() + frame2.GetHashCode();
			}
		}

		public RosSharp.RosBridgeClient.RosConnector rosConnector;
		//private static string tfTopicName = "/tf";
		private static string tfStaticTopicName = "/tf_static";

		private const int MUTEX_TIMEOUT_MILLIS = 5000;
		private System.Collections.Generic.SortedSet<TransformStampedMsg> transforms;
		private System.Collections.Generic.Dictionary<FrameCouple,TransformStampedMsg> staticTransforms;

		//private GenericSubscriber<TfMessageMsg> tfSubscriber;
		private GenericSubscriber<TfMessageMsg> tfStaticSubscriber;
		public long timespanNano = 60*1000000000L;

		private  System.Threading.Mutex mutex = new System.Threading.Mutex();

		private string mutexHeldBy = "none";

        ///<summary>
        /// Unity Start method
        ///</summary>
        void Start ()
		{
			transforms = new System.Collections.Generic.SortedSet<TransformStampedMsg>(new ByTime());
			staticTransforms = new System.Collections.Generic.Dictionary<FrameCouple,TransformStampedMsg>();
			//tfSubscriber = new GenericSubscriber<TfMessageMsg>(rosConnector,tfTopicName);
			//tfSubscriber.addCallback(onTfMessageReceived);
			tfStaticSubscriber = new GenericSubscriber<TfMessageMsg>(rosConnector,tfStaticTopicName);
			tfStaticSubscriber.addCallback(onTfStaticMessageReceived);	
			
		}


        ///<summary>
        ///Called when a /tf message is received
        ///</summary>
  //      void onTfMessageReceived (TfMessageMsg message)
		//{
			
  //      	bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
		//	if(!gotResource)
		//	{
		//		OptLogger.warn("Failed to get mutex (held by "+mutexHeldBy+"), dropping tf message from "+tfTopicName);
		//		return;
		//	}
		//	mutexHeldBy = "onTfMessageReceived";
		//	//OptarLogger.info("received tf message");
		//	foreach(TransformStampedMsg transformStamped in message.transforms)
		//	{
		//		transforms.Add(transformStamped);
		//	}
		//	//remove old data
		//	TransformStampedMsg firstElement = System.Linq.Enumerable.First(transforms);
		//	while(firstElement.header.getTimeNano() < (Utils.getTimeNano() - timespanNano))
		//	{
		//		transforms.Remove(firstElement);
		//		firstElement = System.Linq.Enumerable.First(transforms);
		//	}

		//	mutexHeldBy = "none";
		//	mutex.ReleaseMutex();
		//}

        ///<summary>
        ///Called when a /tf_static message is received
        ///</summary>
        void onTfStaticMessageReceived (TfMessageMsg message)
		{
			bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
			if(!gotResource)
			{
				OptLogger.warn("Failed to get mutex (held by "+mutexHeldBy+"), dropping tf message from "+tfStaticTopicName);
				return;
			}
			mutexHeldBy = "onTfStaticMessageReceived";
			//OptarLogger.info("received tf_static message");
			foreach(TransformStampedMsg transformStamped in message.transforms)
			{
				string source_frame = Utils.cleanFrameId(transformStamped.header.frame_id);
				string target_frame = Utils.cleanFrameId(transformStamped.child_frame_id);
				//OptarLogger.info("adding static transform from "+source_frame+" to "+target_frame);
				FrameCouple key = new FrameCouple(target_frame,source_frame);
				if(staticTransforms.ContainsKey(key))
					staticTransforms.Remove(key);
				staticTransforms.Add(key, transformStamped);
			}
			mutexHeldBy = "none";
			mutex.ReleaseMutex();
		}


        ///<summary>
        ///only works for direct transforms. i.e. tranforms published in a message, it doesn't combine different messages
        ///</summary>
        public TransformMsg lookupTransform(string target_frame, string source_frame, RosSharp.RosBridgeClient.Messages.Standard.Time time)
		{
			
			target_frame = Utils.cleanFrameId(target_frame);
			source_frame = Utils.cleanFrameId(source_frame);

			//OptarLogger.info("lookup transform from "+source_frame+" to "+target_frame+" getting mutex (held by "+mutexHeldBy+")");
			bool gotResource = mutex.WaitOne(MUTEX_TIMEOUT_MILLIS);
			if(!gotResource)
			{
				OptLogger.warn("Failed to get mutex (held by "+mutexHeldBy+"), lookup from "+source_frame+" to "+target_frame+" failed");
				return null;
			}
			mutexHeldBy = "lookupTransform";
			//OptarLogger.info("lookup transform from "+source_frame+" to "+target_frame+": transforms.Count() = "+transforms.Count+", staticTransforms.Count() = "+staticTransforms.Count);

			TransformMsg result = null;
			//if there is a static transform use it
			TransformStampedMsg staticTransform;
			if(staticTransforms.TryGetValue(new FrameCouple(target_frame,source_frame), out staticTransform))
			{
				result =  staticTransform.transform;
			}
			else
			{

				if(Utils.toTimeNano(time)==0)
				{
					//OptarLogger.info("getting last available tf");
					//Search for the transform starting from the last one
					foreach(TransformStampedMsg transformStamped in transforms.Reverse())
					{					
						TransformMsg transform = transformStamped.getTransform(target_frame,source_frame);
						//if we have the transform from source to target
						if(transform!=null)
						{
							result = transform;
							break;
						}
					}
				}
				else
				{
					TransformMsg prevTransform = null;
					long prevTransformTimeNano=-1;
					TransformMsg nextTransform = null;
					long nextTransformTimeNano=-1;
					foreach(TransformStampedMsg transformStamped in transforms)
					{
						//frame_id is source
						//child_frame_id is target_frame
						TransformMsg transform = transformStamped.getTransform(target_frame,source_frame);
						//if we have the transform from source to target
						if(transform!=null)
						{
							//if it is before our time
							if(transformStamped.header.getTimeNano()<Utils.toTimeNano(time))
							{
									prevTransform = transform;
									prevTransformTimeNano = transformStamped.header.getTimeNano();
							}
							else
							{
									nextTransform = transform;
									nextTransformTimeNano = transformStamped.header.getTimeNano();
									break;
							}
						}
					}
					if(prevTransform!=null && nextTransform!=null)
					{
						double weight = (Utils.toTimeNano(time) - prevTransformTimeNano)/(nextTransformTimeNano - prevTransformTimeNano);
						
						// i + (f-i)*w = i + fw -iw = fw + i(1-w)
						Vector3Msg interpolatedPosition = new Vector3Msg(nextTransform.translation.x * weight + prevTransform.translation.x*(1-weight),
																		nextTransform.translation.y * weight + prevTransform.translation.y*(1-weight),
																		nextTransform.translation.z * weight + prevTransform.translation.z*(1-weight));
						

						Quaternion interpolatedQuaternion = Quaternion.Slerp(prevTransform.getOrientation(), nextTransform.getOrientation(), (float)weight);
						result = new TransformMsg(interpolatedPosition,interpolatedQuaternion);
					}
                    else if(prevTransform==null && nextTransform!=null)
                    {
                        throw new ExtrapolationException("Lookup would require extrapolation into the past");
                    }
                    else if(prevTransform!=null && nextTransform==null)
                    { 
                        throw new ExtrapolationException("Lookup would require extrapolation into the future");
                    }
                    else
                    {
                        //both are null
                        throw new LookupException("Require transform not found");
                    }
				}
			}
			mutexHeldBy = "none";
			mutex.ReleaseMutex();
            /*
			if(result==null)
				OptarLogger.info("lookup result is null");
			else
				OptarLogger.info("lookup result is good");
			*/
			return result;
		}


        public Vector3Msg transformPoint(string targe_frame, string source_frame, Vector3Msg pointRos)
        {
            TransformMsg rosToArcoreTransform = lookupTransform(targe_frame, source_frame, Utils.getRosTime(0));
            
            return transformPoint(rosToArcoreTransform,pointRos);
        }

        ///<summary>
        ///Tranforms a point with the provided transform.
        ///Does not throw exceptions
        ///</summary>
        public static Vector3Msg transformPoint(TransformMsg transform, Vector3Msg pointRos)
        {
            //OptarLogger.info("got transform, translation is " + rosToArcoreTransform.getTranslationVector3());
            Matrix4x4 m = Matrix4x4.TRS(transform.getTranslationVector3(),
                                transform.getOrientation(), new Vector3(1, 1, 1));
            Vector3Msg positionRosArcoreFrame = new Vector3Msg(m.inverse.MultiplyPoint(pointRos.asVector3()));
            //OptarLogger.info("positionRosArcore = " + positionRosArcoreFrame);
            return positionRosArcoreFrame;
        }
    }
}
