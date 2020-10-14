<template>
    <f7-page class="my-page">
        <div id="mapContainer" ref="mapCont"/>
<!--        <div class="main-title">Promenade</div>-->
        <MapControls/>
    </f7-page>
</template>

<script>

import { mapState } from 'vuex';

import mapboxgl from 'mapbox-gl';
import MapControls from '../MapControls.vue';

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
        poiMarker: null,
        myMarker: null,
        interval: 0,
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
            return `${this.coordinates.lat}_${this.coordinates.lng}`;
        },
        userName() {
            return this.$store.state.userName;
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
        coordsHash() {
            this.getMyPosition();
            this.getIsochrone();
        },
        userName() {
            this.getMyPosition();
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
            const category = this.$store.getters.getCategory(this.poi.categoryId);
            const node = document.createElement('div');
            node.className = 'flag-container';
            node.innerHTML = `
                <div class="flag">
                    <div class="title">${this.poi.description}</div>
                    <div class="category">
                        <i class="fas fa-${category.icon} icon"></i>${category.name}
                    </div>
                </div>`;

            this.poiMarker = new mapboxgl.Marker(node);
            this.poiMarker.setLngLat([this.poi.coordinates.lng, this.poi.coordinates.lat])
                .addTo(this.map);

            this.flyToMe();
        },
        getMyPosition() {
            if (this.myMarker != null) this.myMarker.remove();
            if (!this.$store.getters.hasCoordinates) return;

            // draw marker with my position
            const node = document.createElement('div');
            node.innerHTML = `
                <div class="my-position">
                    ${this.$store.state.userName[0].toUpperCase()}
                </div>`;

            this.myMarker = new mapboxgl.Marker(node);
            this.myMarker.setLngLat([this.coordinates.lng, this.coordinates.lat])
                .addTo(this.map);

            this.flyToMe();
        },
        flyToMe() {
            // if need move map
            const bounds = this.map.getBounds();
            const me = new mapboxgl.LngLat(this.coordinates.lng, this.coordinates.lat);
            if (!bounds.contains(me)) {
                this.map.flyTo({
                    center: me,
                    zoom: 12,
                });
            }
        },
        checkIfMove() {
            if (!this.$store.getters.hasCoordinates) return;
            this.$store.dispatch('move');
        },
        drawInitialMap() {
            this.getIsochrone();
            this.getRoute();
            this.getPoi();
            this.getMyPosition();
            clearInterval(this.interval);
            this.interval = setInterval(this.checkIfMove, 5000);
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

.my-position {
    background: #3F8AE0;
    box-shadow: 0 2px 3px rgba(0, 0, 0, 0.4);
    width: 20px;
    height: 20px;
    border-radius: 50%;
    font-size: 12px;
    font-weight: 700;
    color: white;
    text-align: center;
    z-index: 3;
}

.main-title {
    position: absolute;
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
</style>
