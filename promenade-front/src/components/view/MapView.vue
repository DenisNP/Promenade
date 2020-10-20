<template>
    <f7-page class="my-page">
        <div id="mapContainer" ref="mapCont"/>
<!--        <div class="main-title">Promenade</div>-->
        <MapControls/>
        <f7-sheet class="my-sheet" swipe-to-close :backdrop="false"
         :opened="!!$store.state.currentPoiInfo" @sheet:closed="closeSheet">
            <div class="title-block">
                Информация
                <font-awesome-icon class="close-icon" icon="times" @click="closeSheet"/>
            </div>
            <f7-page-content>
                <f7-list v-if="!!$store.state.currentPoiInfo" media-list>
                    <f7-list-item
                        v-for="tag in tags"
                        :key="tag.key"
                        :link="constructTagLink(tag)"
                        target="_blank"
                        external
                    >
                    <div slot="header" class="custom-list-item-header">{{tag.key}}</div>
                    <div slot="text" class="custom-list-item-text">{{tag.value}}</div>
                    </f7-list-item>
                </f7-list>
                <div class="empty-block"/>
            </f7-page-content>
        </f7-sheet>

    </f7-page>
</template>

<script>

import { mapState } from 'vuex';

import mapboxgl from 'mapbox-gl';
import MapControls from '../MapControls.vue';
import { firstUpperCase } from '../../utils';
import translateTag from '../../translateTag';

const MapboxLanguage = require('@mapbox/mapbox-gl-language');

export default {
    components: {
        MapControls,
    },
    data: () => ({
        map: null,
        center: [30.315, 59.939],
        zoom: 12,
        mapLoaded: false,
        firstCoordsSet: false,
        poiMarker: null,
        myMarker: null,
        visitedMarkers: [],
        bigIcons: false,
        interval: 0,
        clickedOnce: false,
        clickTimeout: 0,
        accessToken: process.env.VUE_APP_MAPBOX_TOKEN,
        mapOptions: {
            style: 'mapbox://styles/wooferclaw/ckg4xu31c00s019qumnmemqi2',
            center: [30.315, 59.939],
            zoom: 12,
            container: 'mapContainer',
        },
    }),
    computed: {
        ...mapState({
            user: 'user',
            coordinates: 'coordinates',
            poi: 'poi',
            route: 'route',
            range: 'range',
        }),
        showIsochrone() {
            return this.$store.getters.showIsochrone;
        },
        poiId() {
            return this.poi === null ? -1 : this.poi.id;
        },
        coordsHash() {
            if (!this.$store.getters.hasCoordinates) return 'empty';
            return `${this.coordinates.lat}_${this.coordinates.lng}`;
        },
        userName() {
            return this.$store.state.userName;
        },
        category() {
            if (this.poi == null) {
                return {
                    name: '',
                    icon: '',
                };
            }
            return this.$store.getters.getCategory(this.poi.categoryId);
        },
        tags() {
            const poi = this.$store.state.currentPoiInfo;
            if (!poi) return [];
            const category = this.$store.getters.getCategory(poi.categoryId);
            const tags = poi.tags
                .filter(this.filterTags)
                .map((t) => ({
                    key: firstUpperCase(translateTag(t.key)),
                    value: firstUpperCase(t.value),
                    isWiki: t.key.includes('wikipedia'),
                }));

            tags.unshift({ key: 'Координаты', value: `${poi.coordinates.lat}, ${poi.coordinates.lng}`, isCoords: true });
            tags.unshift({ key: 'Категория', value: category.name });
            if (!tags.some((t) => t.value.toLowerCase() === poi.description.toLowerCase())) {
                tags.unshift({ key: 'Название', value: poi.description });
            }

            const unknownTagValues = tags.filter((t) => !t.key).map((t) => t.value);
            const filteredTags = tags.filter((t) => t.key);
            if (unknownTagValues.length > 0) {
                filteredTags.push({ key: 'Прочее', value: unknownTagValues.join('\n') });
            }
            return filteredTags;
        },
        visited() {
            return this.$store.state.visited;
        },
    },
    watch: {
        route() {
            this.getRoute();
        },
        range() {
            this.getIsochrone();
        },
        showIsochrone() {
            this.getIsochrone();
        },
        poiId() {
            this.getPoi();
        },
        coordsHash(newH, oldH) {
            this.getMyPosition(oldH === 'empty');
            this.getIsochrone();
        },
        userName() {
            this.getMyPosition();
        },
        visited() {
            this.getVisited();
        },
        bigIcons(b) {
            if (b) this.$$('.visited-poi').removeClass('vp-small');
            else this.$$('.visited-poi').addClass('vp-small');
        },
    },
    methods: {
        getIsochrone() {
            if (!this.mapLoaded) return;
            if (this.showIsochrone) {
                this.map.setLayoutProperty('isoLayer', 'visibility', 'visible');
                this.map.setLayoutProperty('isoLayerLine', 'visibility', 'visible');
                const urlBase = 'https://api.mapbox.com/isochrone/v1/mapbox/';
                const { lat, lng } = this.coordinates;
                const profile = 'walking';
                const minutes = this.range;
                const query = `${urlBase + profile}/${lng},${lat}?`
                    + `contours_minutes=${minutes}&polygons=true&access_token=${this.accessToken}`;

                fetch(query)
                    .then((response) => response.json())
                    .then((data) => {
                        this.map.getSource('iso').setData(data);
                    })
                    .catch((error) => {
                        console.log(error);
                        // TODO Нормальный вывод алерта или ошибки
                    });
            } else if (this.map.getSource('iso')) {
                this.map.setLayoutProperty('isoLayer', 'visibility', 'none');
                this.map.setLayoutProperty('isoLayerLine', 'visibility', 'none');
            }
        },
        getRoute() {
            if (this.map.getSource('route')) {
                this.map.removeLayer('route');
                this.map.removeSource('route');
            }
            if (
                !this.mapLoaded
                || this.route == null
                || !this.$store.getters.hasCoordinates
            ) return;

            const coordArray = this.route.points.map((item) => [item.lng, item.lat]);

            this.map.addSource('route', {
                type: 'geojson',
                data: {
                    type: 'FeatureCollection',
                    features: [
                        {
                            type: 'Feature',
                            properties: {},
                            geometry: {
                                type: 'LineString',
                                coordinates: coordArray,
                            },
                        },
                    ],
                },
            });

            this.map.addLayer({
                id: 'route',
                type: 'line',
                source: 'route',
                layout: {
                    'line-cap': 'round',
                },
                paint: {
                    'line-color': '#4BB34B',
                    'line-width': 3,
                    'line-dasharray': [0, 2],
                },
            });
        },
        getPoi() {
            if (this.poiMarker != null) this.poiMarker.remove();
            if (this.poi === null) return;

            // draw new poi
            const node = document.createElement('div');
            node.className = 'flag-container';
            node.innerHTML = `
                <div class="flag">
                    <div class="title">${this.poi.description}</div>
                    <div class="category">
                        <i class="fas fa-${this.category.icon} icon"></i>${this.category.name}
                    </div>
                </div>`;

            node.addEventListener('touchend', () => { this.openSheet(); });

            this.poiMarker = new mapboxgl.Marker(node);
            this.poiMarker.setLngLat([this.poi.coordinates.lng, this.poi.coordinates.lat])
                .addTo(this.map);

            this.flyToMe();
        },
        getMyPosition(forceFly) {
            if (this.myMarker != null) this.myMarker.remove();
            if (!this.$store.getters.hasCoordinates) return;

            // draw marker with my position
            const node = document.createElement('div');
            node.className = 'my-position-container';
            node.innerHTML = `
                <div class="my-position">
                    ${firstUpperCase(this.$store.state.userName)[0]}
                </div>`;

            node.addEventListener('touchend', () => { this.forceMove(); });

            this.myMarker = new mapboxgl.Marker(node);
            this.myMarker.setLngLat([this.coordinates.lng, this.coordinates.lat])
                .addTo(this.map);

            this.flyToMe(forceFly);
        },
        getVisited() {
            if (!this.map || !this.mapLoaded) return;
            this.visitedMarkers.forEach((v) => v.remove());
            this.visitedMarkers = [];

            this.visited.forEach((p) => {
                const category = this.$store.getters.getCategory(p.categoryId);
                const node = document.createElement('div');
                node.className = 'visited-poi-container';
                node.innerHTML = `
                    <div class="visited-poi${this.bigIcons ? '' : ' vp-small'}">
                        <i class="fas fa-${category.icon} icon"></i>
                    </div>`;

                node.addEventListener('touchend', () => {
                    this.$store.commit('setCurrentPoiInfo', p);
                });

                const m = new mapboxgl.Marker(node);
                m.setLngLat([p.coordinates.lng, p.coordinates.lat]).addTo(this.map);

                this.visitedMarkers.push(m);
            });
        },
        flyToMe(forceFly) {
            // if need move map
            const bounds = this.map.getBounds();
            const me = new mapboxgl.LngLat(this.coordinates.lng, this.coordinates.lat);
            if (!bounds.contains(me) || forceFly) {
                this.map.flyTo({
                    center: me,
                    zoom: Math.max(12, this.map.getZoom()),
                    animate: this.firstCoordsSet,
                });
            }
            this.firstCoordsSet = true;
        },
        forceMove() {
            this.$store.commit('setCoordinates', { lat: 0, lng: 0 });
            this.$store.dispatch('move');
        },
        constructTagLink(tag) {
            const poi = this.$store.state.currentPoiInfo;
            if (!poi) return null;
            if (tag.isCoords) {
                return `https://yandex.ru/maps/?pt=${poi.coordinates.lng},${poi.coordinates.lat}&z=12&l=map`;
            }
            if (tag.isWiki) {
                return `https://wikipedia.org/wiki/${tag.value}`;
            }
            return null;
        },
        checkIfMove() {
            if (
                !this.$store.getters.hasCoordinates
                || !this.$store.state.networkDisabled
                || !this.$store.state.geoDisabled
                || !this.$store.state.geoDenied
            ) return;
            this.$store.dispatch('move');
        },
        drawInitialMap() {
            this.getIsochrone();
            this.getRoute();
            this.getPoi();
            this.getMyPosition();
            this.getVisited();
            clearInterval(this.interval);
            this.interval = setInterval(this.checkIfMove, 5000);
        },
        openSheet() {
            if (!this.$store.state.currentPoiInfo) {
                window.history.pushState('sheet', null);
            }
            this.$store.commit('setCurrentPoiInfo', this.poi);
        },
        closeSheet() {
            window.history.back();
        },
        filterTags(tag) {
            const cyrillicPattern = /[а-яА-ЯЁё]/;
            const latinPattern = /[a-zA-Z]/;

            return cyrillicPattern.test(tag.value) || !latinPattern.test(tag.value);
        },
    },
    mounted() {
        window.mapboxgl.accessToken = this.accessToken;
        this.map = new window.mapboxgl.Map(this.mapOptions);
        this.map.addControl(new MapboxLanguage({
            defaultLanguage: 'ru',
        }));
        this.map.on('load', () => {
            this.$nextTick(() => {
                this.map.resize();
                this.mapLoaded = true;
                this.drawInitialMap();
            });
            this.map.addSource('iso', {
                type: 'geojson',
                data: {
                    type: 'FeatureCollection',
                    features: [],
                },
            });

            this.map.addLayer({
                id: 'isoLayer',
                type: 'fill',
                source: 'iso',
                layout: {
                    visibility: 'visible',
                },
                paint: {
                    'fill-color': '#459FC6',
                    'fill-opacity': 0.3,
                },
            }, 'poi-label');

            this.map.addLayer({
                id: 'isoLayerLine',
                type: 'line',
                source: 'iso',
                layout: {
                    visibility: 'visible',
                },
                paint: {
                    'line-color': '#0F3B7C',
                    'line-width': 1.5,
                },
            }, 'poi-label');

            this.map.on('zoom', () => {
                const z = this.map.getZoom();
                this.bigIcons = z > 12;
            });
        });
    },

};

</script>

<style>

#mapContainer {
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
}

.flag-container {
    z-index: 2;
}

.flag {
    max-width: 130px;
    min-width: 65px;
    transform: translateY(-45px) translateX(50%);
    box-sizing: border-box;
    background-color: #ffffff;
    border-top: 1px solid #3F8AE0;
    border-bottom: 1px solid #3F8AE0;
    border-right: 1px solid #3F8AE0;
    border-radius: 0 5px 5px 0;
    padding: 5px 10px 5px 10px;
    margin-left: 2px;
    margin-bottom: 24px;
}

.flag .title {
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-size: 10px;
    line-height: 14px;
}

.flag .category {
    font-size: 9px;
    color: #818C99;
}

.flag .category .icon {
    margin-right: 5px;
    font-size: 12px;
    color: #818C99;
}

.flag:before {
    content: '';
    display: block;
    position: absolute;
    left: 0;
    top: 0;
    background-color: #3F8AE0;
    width: 3px;
    height: 90px;
}

.my-position-container {
    /*z-index: 2;*/
}

.my-position {
    background: #3F8AE0;
    box-shadow: 0 2px 3px rgba(0, 0, 0, 0.4);
    width: 24px;
    height: 24px;
    border-radius: 50%;
    font-size: 12px;
    font-weight: 700;
    color: white;
    text-align: center;
    display: flex;
    justify-content: center;
    align-items: center;
}

.main-title {
    position: absolute;
    /*noinspection CssInvalidFunction*/
    top: calc(6px + env(safe-area-inset-top));
    padding-left: 10px;
    height: 40px;
    width: 150px;
    display: flex;
    align-items: center;
    z-index: 2;
    font-size: 24px;
    font-weight: bold;
    background-color: rgba(0,0,0, 0.05);
    color: black;
    border-radius: 0 10px 10px 0;
}

.my-page .page-content {
    padding-top: 0!important;
}

.title-block {
    text-align: center;
    font-size: 18px;
    color: black;
    font-weight: 600;
    position: relative;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.close-icon {
    opacity: 0.15;
    position: absolute;
    right: 14px;
    font-size: 22px;
}

.empty-block {
    width: 100%;
    /*noinspection CssInvalidFunction*/
    /*height: calc(40px + env(safe-area-inset-top));*/
    height: 40px;
}

.custom-list-item-text {
    color: black;
}

.visited-poi {
    font-size: 20px;
    color: #4BB34B;
}

.vp-small {
    font-size: 13px!important;
    color: black!important;
    opacity: 0.3;
}

.custom-list-item-header {
    color: var(--f7-list-item-text-text-color);
}

.my-sheet {
    border-radius: 20px 20px 0 0;
}
</style>
