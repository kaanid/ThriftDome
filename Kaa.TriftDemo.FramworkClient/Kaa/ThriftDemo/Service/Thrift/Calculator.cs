/**
 * Autogenerated by Thrift Compiler (0.11.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace Kaa.ThriftDemo.Service.Thrift
{
  public partial class Calculator {
    /// <summary>
    /// Ahh, now onto the cool part, defining a service. Services just need a name
    /// and can optionally inherit from another service using the extends keyword.
    /// </summary>
    public interface ISync : shared.d.SharedService.ISync {
      /// <summary>
      /// A method definition looks like C code. It has a return type, arguments,
      /// and optionally a list of exceptions that it may throw. Note that argument
      /// lists and exception lists are specified using the exact same syntax as
      /// field lists in struct or exception definitions.
      /// </summary>
      void ping();
      int @add(int num1, int num2);
      int calculate(int logid, Work w);
      /// <summary>
      /// This method has a oneway modifier. That means the client only makes
      /// a request and does not listen for any response at all. Oneway methods
      /// must be void.
      /// </summary>
      void zip();
    }

    /// <summary>
    /// Ahh, now onto the cool part, defining a service. Services just need a name
    /// and can optionally inherit from another service using the extends keyword.
    /// </summary>
    public interface Iface : ISync {
      /// <summary>
      /// A method definition looks like C code. It has a return type, arguments,
      /// and optionally a list of exceptions that it may throw. Note that argument
      /// lists and exception lists are specified using the exact same syntax as
      /// field lists in struct or exception definitions.
      /// </summary>
      #if SILVERLIGHT
      IAsyncResult Begin_ping(AsyncCallback callback, object state);
      void End_ping(IAsyncResult asyncResult);
      #endif
      #if SILVERLIGHT
      IAsyncResult Begin_add(AsyncCallback callback, object state, int num1, int num2);
      int End_add(IAsyncResult asyncResult);
      #endif
      #if SILVERLIGHT
      IAsyncResult Begin_calculate(AsyncCallback callback, object state, int logid, Work w);
      int End_calculate(IAsyncResult asyncResult);
      #endif
      /// <summary>
      /// This method has a oneway modifier. That means the client only makes
      /// a request and does not listen for any response at all. Oneway methods
      /// must be void.
      /// </summary>
      #if SILVERLIGHT
      IAsyncResult Begin_zip(AsyncCallback callback, object state);
      void End_zip(IAsyncResult asyncResult);
      #endif
    }

    /// <summary>
    /// Ahh, now onto the cool part, defining a service. Services just need a name
    /// and can optionally inherit from another service using the extends keyword.
    /// </summary>
    public class Client : shared.d.SharedService.Client, Iface {
      public Client(TProtocol prot) : this(prot, prot)
      {
      }

      public Client(TProtocol iprot, TProtocol oprot) : base(iprot, oprot)
      {
      }

      
      #if SILVERLIGHT
      public IAsyncResult Begin_ping(AsyncCallback callback, object state)
      {
        return send_ping(callback, state);
      }

      public void End_ping(IAsyncResult asyncResult)
      {
        oprot_.Transport.EndFlush(asyncResult);
        recv_ping();
      }

      #endif

      /// <summary>
      /// A method definition looks like C code. It has a return type, arguments,
      /// and optionally a list of exceptions that it may throw. Note that argument
      /// lists and exception lists are specified using the exact same syntax as
      /// field lists in struct or exception definitions.
      /// </summary>
      public void ping()
      {
        #if !SILVERLIGHT
        send_ping();
        recv_ping();

        #else
        var asyncResult = Begin_ping(null, null);
        End_ping(asyncResult);

        #endif
      }
      #if SILVERLIGHT
      public IAsyncResult send_ping(AsyncCallback callback, object state)
      #else
      public void send_ping()
      #endif
      {
        oprot_.WriteMessageBegin(new TMessage("ping", TMessageType.Call, seqid_));
        ping_args args = new ping_args();
        args.Write(oprot_);
        oprot_.WriteMessageEnd();
        #if SILVERLIGHT
        return oprot_.Transport.BeginFlush(callback, state);
        #else
        oprot_.Transport.Flush();
        #endif
      }

      public void recv_ping()
      {
        TMessage msg = iprot_.ReadMessageBegin();
        if (msg.Type == TMessageType.Exception) {
          TApplicationException x = TApplicationException.Read(iprot_);
          iprot_.ReadMessageEnd();
          throw x;
        }
        ping_result result = new ping_result();
        result.Read(iprot_);
        iprot_.ReadMessageEnd();
        return;
      }

      
      #if SILVERLIGHT
      public IAsyncResult Begin_add(AsyncCallback callback, object state, int num1, int num2)
      {
        return send_add(callback, state, num1, num2);
      }

      public int End_add(IAsyncResult asyncResult)
      {
        oprot_.Transport.EndFlush(asyncResult);
        return recv_add();
      }

      #endif

      public int @add(int num1, int num2)
      {
        #if !SILVERLIGHT
        send_add(num1, num2);
        return recv_add();

        #else
        var asyncResult = Begin_add(null, null, num1, num2);
        return End_add(asyncResult);

        #endif
      }
      #if SILVERLIGHT
      public IAsyncResult send_add(AsyncCallback callback, object state, int num1, int num2)
      #else
      public void send_add(int num1, int num2)
      #endif
      {
        oprot_.WriteMessageBegin(new TMessage("add", TMessageType.Call, seqid_));
        add_args args = new add_args();
        args.Num1 = num1;
        args.Num2 = num2;
        args.Write(oprot_);
        oprot_.WriteMessageEnd();
        #if SILVERLIGHT
        return oprot_.Transport.BeginFlush(callback, state);
        #else
        oprot_.Transport.Flush();
        #endif
      }

      public int recv_add()
      {
        TMessage msg = iprot_.ReadMessageBegin();
        if (msg.Type == TMessageType.Exception) {
          TApplicationException x = TApplicationException.Read(iprot_);
          iprot_.ReadMessageEnd();
          throw x;
        }
        add_result result = new add_result();
        result.Read(iprot_);
        iprot_.ReadMessageEnd();
        if (result.__isset.success) {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "add failed: unknown result");
      }

      
      #if SILVERLIGHT
      public IAsyncResult Begin_calculate(AsyncCallback callback, object state, int logid, Work w)
      {
        return send_calculate(callback, state, logid, w);
      }

      public int End_calculate(IAsyncResult asyncResult)
      {
        oprot_.Transport.EndFlush(asyncResult);
        return recv_calculate();
      }

      #endif

      public int calculate(int logid, Work w)
      {
        #if !SILVERLIGHT
        send_calculate(logid, w);
        return recv_calculate();

        #else
        var asyncResult = Begin_calculate(null, null, logid, w);
        return End_calculate(asyncResult);

        #endif
      }
      #if SILVERLIGHT
      public IAsyncResult send_calculate(AsyncCallback callback, object state, int logid, Work w)
      #else
      public void send_calculate(int logid, Work w)
      #endif
      {
        oprot_.WriteMessageBegin(new TMessage("calculate", TMessageType.Call, seqid_));
        calculate_args args = new calculate_args();
        args.Logid = logid;
        args.W = w;
        args.Write(oprot_);
        oprot_.WriteMessageEnd();
        #if SILVERLIGHT
        return oprot_.Transport.BeginFlush(callback, state);
        #else
        oprot_.Transport.Flush();
        #endif
      }

      public int recv_calculate()
      {
        TMessage msg = iprot_.ReadMessageBegin();
        if (msg.Type == TMessageType.Exception) {
          TApplicationException x = TApplicationException.Read(iprot_);
          iprot_.ReadMessageEnd();
          throw x;
        }
        calculate_result result = new calculate_result();
        result.Read(iprot_);
        iprot_.ReadMessageEnd();
        if (result.__isset.success) {
          return result.Success;
        }
        if (result.__isset.ouch) {
          throw result.Ouch;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "calculate failed: unknown result");
      }

      
      #if SILVERLIGHT
      public IAsyncResult Begin_zip(AsyncCallback callback, object state)
      {
        return send_zip(callback, state);
      }

      public void End_zip(IAsyncResult asyncResult)
      {
        oprot_.Transport.EndFlush(asyncResult);
      }

      #endif

      /// <summary>
      /// This method has a oneway modifier. That means the client only makes
      /// a request and does not listen for any response at all. Oneway methods
      /// must be void.
      /// </summary>
      public void zip()
      {
        #if !SILVERLIGHT
        send_zip();

        #else
        var asyncResult = Begin_zip(null, null);

        #endif
      }
      #if SILVERLIGHT
      public IAsyncResult send_zip(AsyncCallback callback, object state)
      #else
      public void send_zip()
      #endif
      {
        oprot_.WriteMessageBegin(new TMessage("zip", TMessageType.Oneway, seqid_));
        zip_args args = new zip_args();
        args.Write(oprot_);
        oprot_.WriteMessageEnd();
        #if SILVERLIGHT
        return oprot_.Transport.BeginFlush(callback, state);
        #else
        oprot_.Transport.Flush();
        #endif
      }

    }
    public class Processor : shared.d.SharedService.Processor, TProcessor {
      public Processor(ISync iface) : base(iface)
      {
        iface_ = iface;
        processMap_["ping"] = ping_Process;
        processMap_["add"] = add_Process;
        processMap_["calculate"] = calculate_Process;
        processMap_["zip"] = zip_Process;
      }

      private ISync iface_;

      public new bool Process(TProtocol iprot, TProtocol oprot)
      {
        try
        {
          TMessage msg = iprot.ReadMessageBegin();
          ProcessFunction fn;
          processMap_.TryGetValue(msg.Name, out fn);
          if (fn == null) {
            TProtocolUtil.Skip(iprot, TType.Struct);
            iprot.ReadMessageEnd();
            TApplicationException x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            oprot.WriteMessageBegin(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID));
            x.Write(oprot);
            oprot.WriteMessageEnd();
            oprot.Transport.Flush();
            return true;
          }
          fn(msg.SeqID, iprot, oprot);
        }
        catch (IOException)
        {
          return false;
        }
        return true;
      }

      public void ping_Process(int seqid, TProtocol iprot, TProtocol oprot)
      {
        ping_args args = new ping_args();
        args.Read(iprot);
        iprot.ReadMessageEnd();
        ping_result result = new ping_result();
        try
        {
          iface_.ping();
          oprot.WriteMessageBegin(new TMessage("ping", TMessageType.Reply, seqid)); 
          result.Write(oprot);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          TApplicationException x = new TApplicationException        (TApplicationException.ExceptionType.InternalError," Internal error.");
          oprot.WriteMessageBegin(new TMessage("ping", TMessageType.Exception, seqid));
          x.Write(oprot);
        }
        oprot.WriteMessageEnd();
        oprot.Transport.Flush();
      }

      public void add_Process(int seqid, TProtocol iprot, TProtocol oprot)
      {
        add_args args = new add_args();
        args.Read(iprot);
        iprot.ReadMessageEnd();
        add_result result = new add_result();
        try
        {
          result.Success = iface_.@add(args.Num1, args.Num2);
          oprot.WriteMessageBegin(new TMessage("add", TMessageType.Reply, seqid)); 
          result.Write(oprot);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          TApplicationException x = new TApplicationException        (TApplicationException.ExceptionType.InternalError," Internal error.");
          oprot.WriteMessageBegin(new TMessage("add", TMessageType.Exception, seqid));
          x.Write(oprot);
        }
        oprot.WriteMessageEnd();
        oprot.Transport.Flush();
      }

      public void calculate_Process(int seqid, TProtocol iprot, TProtocol oprot)
      {
        calculate_args args = new calculate_args();
        args.Read(iprot);
        iprot.ReadMessageEnd();
        calculate_result result = new calculate_result();
        try
        {
          try
          {
            result.Success = iface_.calculate(args.Logid, args.W);
          }
          catch (InvalidOperation ouch)
          {
            result.Ouch = ouch;
          }
          oprot.WriteMessageBegin(new TMessage("calculate", TMessageType.Reply, seqid)); 
          result.Write(oprot);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          TApplicationException x = new TApplicationException        (TApplicationException.ExceptionType.InternalError," Internal error.");
          oprot.WriteMessageBegin(new TMessage("calculate", TMessageType.Exception, seqid));
          x.Write(oprot);
        }
        oprot.WriteMessageEnd();
        oprot.Transport.Flush();
      }

      public void zip_Process(int seqid, TProtocol iprot, TProtocol oprot)
      {
        zip_args args = new zip_args();
        args.Read(iprot);
        iprot.ReadMessageEnd();
        try
        {
          iface_.zip();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
        }
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class ping_args : TBase
    {

      public ping_args() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("ping_args");
          oprot.WriteStructBegin(struc);
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("ping_args(");
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class ping_result : TBase
    {

      public ping_result() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("ping_result");
          oprot.WriteStructBegin(struc);

          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("ping_result(");
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class add_args : TBase
    {
      private int _num1;
      private int _num2;

      public int Num1
      {
        get
        {
          return _num1;
        }
        set
        {
          __isset.num1 = true;
          this._num1 = value;
        }
      }

      public int Num2
      {
        get
        {
          return _num2;
        }
        set
        {
          __isset.num2 = true;
          this._num2 = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool num1;
        public bool num2;
      }

      public add_args() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.I32) {
                  Num1 = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              case 2:
                if (field.Type == TType.I32) {
                  Num2 = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("add_args");
          oprot.WriteStructBegin(struc);
          TField field = new TField();
          if (__isset.num1) {
            field.Name = "num1";
            field.Type = TType.I32;
            field.ID = 1;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Num1);
            oprot.WriteFieldEnd();
          }
          if (__isset.num2) {
            field.Name = "num2";
            field.Type = TType.I32;
            field.ID = 2;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Num2);
            oprot.WriteFieldEnd();
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("add_args(");
        bool __first = true;
        if (__isset.num1) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Num1: ");
          __sb.Append(Num1);
        }
        if (__isset.num2) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Num2: ");
          __sb.Append(Num2);
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class add_result : TBase
    {
      private int _success;

      public int Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool success;
      }

      public add_result() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.I32) {
                  Success = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("add_result");
          oprot.WriteStructBegin(struc);
          TField field = new TField();

          if (this.__isset.success) {
            field.Name = "Success";
            field.Type = TType.I32;
            field.ID = 0;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Success);
            oprot.WriteFieldEnd();
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("add_result(");
        bool __first = true;
        if (__isset.success) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Success: ");
          __sb.Append(Success);
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class calculate_args : TBase
    {
      private int _logid;
      private Work _w;

      public int Logid
      {
        get
        {
          return _logid;
        }
        set
        {
          __isset.logid = true;
          this._logid = value;
        }
      }

      public Work W
      {
        get
        {
          return _w;
        }
        set
        {
          __isset.w = true;
          this._w = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool logid;
        public bool w;
      }

      public calculate_args() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 1:
                if (field.Type == TType.I32) {
                  Logid = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              case 2:
                if (field.Type == TType.Struct) {
                  W = new Work();
                  W.Read(iprot);
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("calculate_args");
          oprot.WriteStructBegin(struc);
          TField field = new TField();
          if (__isset.logid) {
            field.Name = "logid";
            field.Type = TType.I32;
            field.ID = 1;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Logid);
            oprot.WriteFieldEnd();
          }
          if (W != null && __isset.w) {
            field.Name = "w";
            field.Type = TType.Struct;
            field.ID = 2;
            oprot.WriteFieldBegin(field);
            W.Write(oprot);
            oprot.WriteFieldEnd();
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("calculate_args(");
        bool __first = true;
        if (__isset.logid) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Logid: ");
          __sb.Append(Logid);
        }
        if (W != null && __isset.w) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("W: ");
          __sb.Append(W== null ? "<null>" : W.ToString());
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class calculate_result : TBase
    {
      private int _success;
      private InvalidOperation _ouch;

      public int Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }

      public InvalidOperation Ouch
      {
        get
        {
          return _ouch;
        }
        set
        {
          __isset.ouch = true;
          this._ouch = value;
        }
      }


      public Isset __isset;
      #if !SILVERLIGHT
      [Serializable]
      #endif
      public struct Isset {
        public bool success;
        public bool ouch;
      }

      public calculate_result() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              case 0:
                if (field.Type == TType.I32) {
                  Success = iprot.ReadI32();
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              case 1:
                if (field.Type == TType.Struct) {
                  Ouch = new InvalidOperation();
                  Ouch.Read(iprot);
                } else { 
                  TProtocolUtil.Skip(iprot, field.Type);
                }
                break;
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("calculate_result");
          oprot.WriteStructBegin(struc);
          TField field = new TField();

          if (this.__isset.success) {
            field.Name = "Success";
            field.Type = TType.I32;
            field.ID = 0;
            oprot.WriteFieldBegin(field);
            oprot.WriteI32(Success);
            oprot.WriteFieldEnd();
          } else if (this.__isset.ouch) {
            if (Ouch != null) {
              field.Name = "Ouch";
              field.Type = TType.Struct;
              field.ID = 1;
              oprot.WriteFieldBegin(field);
              Ouch.Write(oprot);
              oprot.WriteFieldEnd();
            }
          }
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("calculate_result(");
        bool __first = true;
        if (__isset.success) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Success: ");
          __sb.Append(Success);
        }
        if (Ouch != null && __isset.ouch) {
          if(!__first) { __sb.Append(", "); }
          __first = false;
          __sb.Append("Ouch: ");
          __sb.Append(Ouch== null ? "<null>" : Ouch.ToString());
        }
        __sb.Append(")");
        return __sb.ToString();
      }

    }


    #if !SILVERLIGHT
    [Serializable]
    #endif
    public partial class zip_args : TBase
    {

      public zip_args() {
      }

      public void Read (TProtocol iprot)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          TField field;
          iprot.ReadStructBegin();
          while (true)
          {
            field = iprot.ReadFieldBegin();
            if (field.Type == TType.Stop) { 
              break;
            }
            switch (field.ID)
            {
              default: 
                TProtocolUtil.Skip(iprot, field.Type);
                break;
            }
            iprot.ReadFieldEnd();
          }
          iprot.ReadStructEnd();
        }
        finally
        {
          iprot.DecrementRecursionDepth();
        }
      }

      public void Write(TProtocol oprot) {
        oprot.IncrementRecursionDepth();
        try
        {
          TStruct struc = new TStruct("zip_args");
          oprot.WriteStructBegin(struc);
          oprot.WriteFieldStop();
          oprot.WriteStructEnd();
        }
        finally
        {
          oprot.DecrementRecursionDepth();
        }
      }

      public override string ToString() {
        StringBuilder __sb = new StringBuilder("zip_args(");
        __sb.Append(")");
        return __sb.ToString();
      }

    }

  }
}