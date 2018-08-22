namespace java shared.d
namespace csharp shared.d
namespace netcore shared.d

struct SharedStruct {
  1: i32 key
  2: string value
}

service SharedService {
  SharedStruct getStruct(1: i32 key);
}