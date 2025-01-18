angular.module("umbraco")
    .controller("ImageCrop.MediaCropDetailsController",
        function ($scope, mediaResource) {

            var vm = this;
            init();

            function init() {
                vm.uniqueId = String.CreateGuid();
                vm.cnt = 1;

                vm.submit = submit;
                vm.close = close;
                vm.selectCrop = selectCrop;
                vm.reset = reset;

                vm.model = $scope.model;
                vm.caption = vm.model.target.caption;
                vm.altText = vm.model.target.altText;
                vm.imageSrc = $scope.model.target.image ?? $scope.model.target.src;
                if (!vm.imageSrc) { return; }
                vm.imageSrcOrig = vm.imageSrc;
                vm.delimiter = vm.imageSrc.indexOf("?") < 0 ? "?" : "&";

                vm.focalPoint = { left: .5, top: .5 };
                vm.hasCrops = cropSet() === true;
            }

            function reset() {
                var cropOrigin = vm.Crops.find(c => c.alias === vm.selectedCrop.alias)
                if (cropOrigin) {
                    vm.selectedCrop = deepCopy(cropOrigin);
                    vm.cnt++;
                    vm.imageSrc = vm.imageSrcOrig + vm.delimiter + "cnt=" + vm.cnt;
                    UpdateCropSettings();
                }
            }

            function submit() {
                if ($scope.model && $scope.model.submit) {
                    $scope.model.target.selectedCrop = vm.selectedCrop;
                    vm.model.target.caption = vm.caption;
                    vm.model.target.altText = vm.altText;
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if ($scope.model && $scope.model.close) {
                    $scope.model.close($scope.model);
                }
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
                if (!vm.altText && !vm.model.target.altText && vm.model.target.altText !== '') {
                    vm.altText = mediaItem.name;
                }
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

            function selectCrop(cropAlias) {
                var crop = $scope.model.target.selectedCrop;
                if (!crop || crop.alias !== cropAlias) {
                    crop = vm.Crops.find(c => c.alias === cropAlias);
                }
                vm.selectedCrop = deepCopy(crop);
                UpdateCropSettings();
            }

            function UpdateCropSettings() {
                var crop = vm.selectedCrop;
                vm.selectedCropAlias = crop.alias;
                vm.hasCrops = !!crop;
                vm.cropSize = {
                    width: crop.width,
                    height: crop.height
                };
            }

            function deepCopy(obj) {
                if (!obj) { return null }

                return JSON.parse(JSON.stringify(obj));
            }
        });
