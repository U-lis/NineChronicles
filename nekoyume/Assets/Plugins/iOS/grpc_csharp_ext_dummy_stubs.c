
// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// When building for Unity Android with il2cpp backend, Unity tries to link
// the __Internal PInvoke definitions (which are required by iOS) even though
// the .so/.dll will be actually used. This file provides dummy stubs to
// make il2cpp happy.
// See https://github.com/grpc/grpc/issues/16012

#include <stdio.h>
#include <stdlib.h>

void deflate() {
  fprintf(stderr, "Should never reach here");
  abort();
}
void deflateEnd() {
  fprintf(stderr, "Should never reach here");
  abort();
}
void deflateInit2_() {
  fprintf(stderr, "Should never reach here");
  abort();
}
void inflate() {
  fprintf(stderr, "Should never reach here");
  abort();
}
void inflateInit2_() {
  fprintf(stderr, "Should never reach here");
  abort();
}

void inflateEnd(){
	fprintf(stderr, "Should never reach here");
}
