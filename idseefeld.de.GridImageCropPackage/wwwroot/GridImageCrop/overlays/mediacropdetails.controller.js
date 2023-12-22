angular.module("umbraco")
    .controller("ImageCrop.MediaCropDetailsController",
        function ($scope, mediaResource, mediaTypeResource) {

            var vm = this;

            vm.uniqueId = String.CreateGuid();

            vm.submit = submit;
            vm.close = close;
            vm.hasCrops = cropSet() === true;
            vm.focalPointChanged = focalPointChanged;
            vm.toggleFocalPoint = toggleFocalPoint;
            vm.disableFocalPoint = false;
            vm.selectCrop = selectCrop;
            vm.model = $scope.model;            
            vm.imageSrc = $scope.model.target.image ?? $scope.model.target.src;

            if (typeof $scope.model.disableFocalPoint === "boolean") {
                vm.disableFocalPoint = $scope.model.disableFocalPoint
            }
            else {
                vm.disableFocalPoint = ($scope.model.disableFocalPoint !== undefined && $scope.model.disableFocalPoint !== "0") ? true : false;
            }

            if (!$scope.model.target.coordinates && !$scope.model.target.focalPoint) {
                $scope.model.target.focalPoint = { left: .5, top: .5 };
            }

            if (!$scope.model.target
                || $scope.model.target.id
                || ($scope.model.target.src && $scope.model.target.src.toLowerCase().startsWith("blob:"))) {
                vm.shouldShowUrl = false;
            } else {
                vm.shouldShowUrl = true;
            }

            /**
             * Called when the umbImageGravity component updates the focal point value
             * @param {any} left
             * @param {any} top
             */
            function focalPointChanged(left, top) {
                // update the model focalpoint value
                $scope.model.target.focalPoint = {
                    left: left,
                    top: top
                };
            }

            function selectCrop(cropAlias) {
                const model = $scope.model;
                const crop = vm.Crops.find(c => c.alias === cropAlias)
                UpdateCropSettings(crop);
                if (!crop) {
                    model.target.thumbnail = getImageSrc();
                } else {
                    model.target.thumbnail = getImageSrc(crop);
                }
            }

            function toggleFocalPoint() {
                $scope.model.target.disableFocalPoint = vm.disableFocalPoint;
            }

            function submit() {
                if ($scope.model && $scope.model.submit) {
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if ($scope.model && $scope.model.close) {
                    $scope.model.close($scope.model);
                }
            }

            function getImageSrc(crop) {
                var target = $scope.model.target;
                var src = target.src;
                if (crop) {
                    src += '?width=' + crop.width + '&height=' + crop.height;
                } else {
                    src += '?width=' + target.width + '&height=' + target.height;
                }
                return src;
            }

            function cropSet() {
                var model = $scope.model;
                if (!model.target) {
                    return false;
                }

                var mediaItem = model.mediaItem;
                if (!mediaItem?.properties) {
                    mediaResource.getById(model.target.id)
                        .then(function (data) {
                            setCropsFromMediaItem(data);
                        });
                } else {
                    setCropsFromMediaItem(mediaItem);
                    return vm.hasCrops;
                }

                return false;
            }

            function setCropsFromMediaItem(mediaItem) {
                var image = mediaItem?.properties?.find(p => p.editor === "Umbraco.ImageCropper");
                var crops = image?.value?.crops;
                var defaultCrops = image?.config?.crops.map(c => c.alias);
                if (crops && defaultCrops) {
                    vm.CropAliasSelection = defaultCrops;
                    vm.Crops = crops;
                    var crop = $scope.model.target.selectedCrop;
                    if (!crop && crops?.length) {
                        crop = crops[0];
                    }
                    selectCrop(crop.alias);
                }
            }

            function UpdateCropSettings(crop) {
                $scope.model.target.selectedCrop = crop;
                vm.selectedCropAlias = crop.alias;
                vm.hasCrops = !!crop;
                $scope.model.target.coordinates = crop.coordinates;
                $scope.model.cropSize = {
                    width: crop.width,
                    height: crop.height
                };
            }
        });
