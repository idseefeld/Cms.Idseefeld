(function () {
    "use strict";
    function ImageCropController($scope, userService, editorService, localizationService, mediaResource) {

        $scope.control.icon = $scope.control.icon || 'icon-picture';

        if (!$scope.model.config.startNodeId) {
            if ($scope.model.config.ignoreUserStartNodes === true) {
                $scope.model.config.startNodeId = -1;
                $scope.model.config.startNodeIsVirtual = true;

            } else {
                userService.getCurrentUser().then(userData => {
                    $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                    $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                });
            }
        }

        $scope.setMediaItem = function () {
            var startNodeId = $scope.model.config && $scope.model.config.startNodeId ? $scope.model.config.startNodeId : null;

            var mediaPicker = {
                startNodeId: startNodeId,
                startNodeIsVirtual: startNodeId ? $scope.model.config.startNodeIsVirtual : null,
                //cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : null,
                showDetails: true,
                disableFolderSelect: true,
                onlyImages: true,
                dataTypeKey: $scope.model.dataTypeKey,
                size: "medium",
                view: "/App_Plugins/GridImageCrop/mediapicker.html",
                submit: model => {
                    setControlValue(model.selection[0]);
                    editorService.close();
                },
                close: () => editorService.close()
            };

            editorService.open(mediaPicker);
        };

        $scope.editProperties = function () {

            const mediaCropDetailsConfig = {
                size: 'small',
                target: $scope.control.value,
                mediaItem: $scope.mediaItem,
                view: "/App_Plugins/GridImageCrop/overlays/mediacropdetails.html",
                submit: model => {
                    updateControlValue(model.target);
                    editorService.close();
                },
                close: () => editorService.close()
            };

            localizationService.localize('defaultdialogs_editSelectedMedia').then(value => {
                mediaCropDetailsConfig.title = value;
                editorService.open(mediaCropDetailsConfig);
            });
        }

        if ($scope.control?.value) {
            getMediaById();
        }

        function getMediaById() {
            try {
                if ($scope.control.value?.id) {
                    return mediaResource.getById($scope.control.value.id)
                        .then(function (data) {
                            $scope.mediaItem = data;
                            updateControlValue();
                        }, function (err) {
                            console.log(err);
                        });
                }
            } catch (err) {
                if (err) {
                    console.error(err);
                }
            }
        }

        function setControlValue(data) {
            const newValue = {
                focalPoint: data.focalPoint,
                id: data.id,
                caption: data.caption,
                altText: data.altText,
                selectedCrop: data.selectedCrop,
                disableFocalPoint: data.disableFocalPoint ?? false,
                coordinates: data.coordinates
            }
            $scope.control.value = newValue;
            getMediaById();
        }

        function updateControlValue(data) {
            var media = $scope.mediaItem;
            var w = parseInt(media.properties?.find(p => p.alias === "umbracoWidth")?.value);
            var h = parseInt(media.properties?.find(p => p.alias === "umbracoHeight")?.value);
            var image = media.properties?.find(p => p.editor === "Umbraco.ImageCropper");
            if (!image || !image.value) {
                console.error("no Umbraco.ImageCropper")
                return;
            }

            if(!data){
                data = $scope.control.value;
            }
            var updatedValue = {
                focalPoint: data.focalPoint,
                id: data.id,
                caption: data.caption,
                altText: data.altText,
                selectedCrop: data.selectedCrop,
                disableFocalPoint: data.disableFocalPoint ?? false,
                coordinates: data.coordinates
            }
            updatedValue.coordinates = data.coordinates
            updatedValue.selectedCrop = data.selectedCrop,
            updatedValue.udi = media.udi;
            updatedValue.width = w;
            updatedValue.height = h;
            updatedValue.src = image.value.src;
            if (image.value.crops) {
                $scope.Crops = image.value.crops;
            }
            $scope.control.value = updatedValue;

            $scope.thumbnailUrl = getThumbnailUrl(updatedValue);
            $scope.caption = updatedValue.caption;
            $scope.altText = updatedValue.altText ?? media.name;
        }

        function getThumbnailUrl(updatedValue) {
            if (updatedValue && updatedValue.selectedCrop) {
                var url = updatedValue.src;
                var crop = updatedValue.selectedCrop;
                if (crop) {
                    var maxSize = 1200;
                    var w = crop.width;
                    var h = crop.height;

                    if (crop.coordinates) {
                        var coords = crop.coordinates;
                        url += `?cc=${coords.x1},${coords.y1},${coords.x2},${coords.y2}`;
                    } else if (updatedValue.focalPoint) {
                        url += `?rxy=${updatedValue.focalPoint.left},${updatedValue.focalPoint.top}`;
                    } else {
                        url += '?rxy=0.5,0.5';
                    }

                    var ratio = w / h;
                    if (w > maxSize) {
                        w = maxSize;
                        h = w / ratio;
                    }
                    if (h > maxSize) {
                        h = maxSize;
                        w = h * ratio;
                    }

                    url += '&width=' + w;
                    url += '&height=' + h;
                }

                // set default size if no crop present (moved from the view)
                if (url.includes('?') === false) {
                    url += '?width=' + maxSize;
                }

                return url;
            }

            return null;
        }
    }

    angular.module("umbraco").controller("imageCropGridEditor.Controller", ImageCropController);
})();