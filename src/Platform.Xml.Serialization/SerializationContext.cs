using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Maintains state required to perform serialization.
	/// </summary>
	public sealed class SerializationContext
	{
		private readonly IList stack = new ArrayList();
		public SerializationParameters Parameters { get; }
		public SerializerOptions SerializerOptions { get; set; }

		public SerializationContext(SerializerOptions options, SerializationParameters parameters)
		{			
			this.Parameters = parameters;
			SerializerOptions = options;
		}
		
		/// <summary>
		/// Checks if see if an object should be serialized.
		/// </summary>
		/// <remarks>
		/// <p>
		/// An object shouldn't be serialized if it has already been serialized.
		/// This method automatically checks if the object has been serialized
		/// by examining the serialization stack.  This stack is maintained by
		/// the SerializationStart and SerializationEnd methods.
		/// </p>
		/// <p>
		/// You should call SerializationStart and SerializationEnd when you start
		/// and finish serializing an object.
		/// </p>
		/// </remarks>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool ShouldSerialize(object obj, SerializationMemberInfo memberInfo)
		{
			IXmlSerializationShouldSerializeProvider shouldSerialize;

			if (obj == null && !memberInfo.SerializeIfNull)
			{
				return false;
			}

			if ((shouldSerialize = obj as IXmlSerializationShouldSerializeProvider) != null)
			{
				if (!shouldSerialize.ShouldSerialize(this.SerializerOptions, this.Parameters))
				{
					return false;
				}
			}

			for (var i = 0; i < stack.Count; i++)
			{
				if (stack[i] == obj)
				{
					return false;
				}
			}

			return true;
		}

		private readonly Stack serializationMemberInfoStack = new Stack();

		public void PushCurrentMemberInfo(SerializationMemberInfo memberInfo)
		{
			serializationMemberInfoStack.Push(memberInfo);
		}

		public void PopCurrentMemberInfo()
		{
			serializationMemberInfoStack.Pop();
		}

		public SerializationMemberInfo GetCurrentMemberInfo()
		{
			return (SerializationMemberInfo)serializationMemberInfoStack.Peek();
		}

		public void DeserializationStart(object obj)
		{
			var listener = obj as IXmlDeserializationStartListener;

			listener?.XmlDeserializationStart(this.Parameters);
		}

		public void DeserializationEnd(object obj)
		{
			var listener = obj as IXmlDeserializationEndListener;

			listener?.XmlDeserializationEnd(this.Parameters);
		}

		/// <summary>
		/// Prepares an object for serialization/
		/// </summary>
		/// <remarks>
		/// The object is pushed onto the serialization stack. 
		/// This prevents the object from being serialized in cycles.
		/// </remarks>
		/// <param name="obj"></param>
		public void SerializationStart(object obj)
		{
			stack.Add(obj);

			var listener = obj as IXmlSerializationStartListener;

			listener?.XmlSerializationStart(this.Parameters);
		}

		/// <summary>
		/// Call when an object has been serialized.
		/// </summary>
		/// <remarks>
		/// The object is popped off the serialization stack.		
		/// </remarks>
		/// <param name="obj"></param>
		public void SerializationEnd(object obj)
		{
			if (stack[stack.Count - 1] != obj)
			{
				stack.RemoveAt(stack.Count - 1);

				throw new InvalidOperationException("Push/Pop misalignment.");
			}

			stack.RemoveAt(stack.Count - 1);

			var listener = obj as IXmlSerializationEndListener;

			listener?.XmlSerializationEnd(this.Parameters);
		}
	}
}
