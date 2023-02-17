#!/usr/bin/env bash

set -ex

UNITY_VERSION=2021.3.5f1
GAME_CI_VERSION=1.0.1 # https://github.com/game-ci/docker/releases
MY_USERNAME=atralupus
GAME_CI_UNITY_EDITOR_IMAGE="unityci/editor:windows-2021.3.5f1-windows-il2cpp-1"

# don't hesitate to remove unused components from this list
declare -a components=("android")

for component in "${components[@]}"
do
  IMAGE_TO_PUBLISH=${MY_USERNAME}/editor:windows-${UNITY_VERSION}-${component}-${GAME_CI_VERSION}
  docker build --build-arg GAME_CI_UNITY_EDITOR_IMAGE=${GAME_CI_UNITY_EDITOR_IMAGE} --build-arg module=${component} . -t ${IMAGE_TO_PUBLISH}
# uncomment the following to publish the built images to docker hub
#  docker push ${IMAGE_TO_PUBLISH}
done
