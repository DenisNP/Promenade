<template>
    <f7-page>
        <div id="mapContainer" ref="mapCont" :style="mapSize" v-show="true" key="mapDiv"></div>
        <MapControls/>
    </f7-page>
</template>

<script>

import { mapGetters, mapActions, mapMutations } from 'vuex';

import MapControls from '../MapControls.vue';

const MapboxLanguage = require('@mapbox/mapbox-gl-language');

console.log(process.env.MAPBOX_TOKEN);

export default {
    components: {
        MapControls,
    },
    data: () => ({
        map: null,
        center: [30.315, 59.939],
        zoom: 12,
        mapSize: '',
        mapLoaded: false,
        accessToken: process.env.VUE_APP_MAPBOX_TOKEN,
        mapOptions: {
            style: 'mapbox://styles/wooferclaw/ckfqkf60k0a0l19nr455yeeu4',
            center: [30.315, 59.939],
            zoom: 12,
            container: 'mapContainer',
        },
    }),
    computed: {
        ...mapGetters({
            mapState: 'mapState',
            User: 'user',
            Isochrone: 'isochrone',
            Coordinates: 'coordinates',
        }),
        isochroneZone() {
            return this.Isochrone.zone;
        },
    },
    watch: {
        mapState(state) {
            console.log('Я новое состояние ', state);

            switch (state) {
            case 'isochrone':
                this.getIsochrone(this.Isochrone.zone);
                break;
            case 'route':
                /** Тут код для рисования маршрута
                 *  по параметрам из this.MapData
                 *  (как вы там договоритесь их хранить) */
                break;
            case 'finish':
                /** Тут код для финиша
                 *  по параметрам из this.MapData
                 *  (как вы там договоритесь их хранить) */
                break;
                /* Тут всякие другие случаи */

            default: console.warn('Но кажется это состояние забыли учесть');
            }
        },
        isochroneZone(zone) {
            this.getIsochrone(zone);
        },
    },
    methods: {
        ...mapActions([
            'settingsShow',
        ]),
        ...mapMutations({
            mapStateChange: 'mapStateToggle',
        }),
        getIsochrone(zone) {
            const urlBase = 'https://api.mapbox.com/isochrone/v1/mapbox/';
            const lng = 30.315;
            const lat = 59.939;
            // const { lat, lng } = this.Coordinates;
            const profile = 'walking';
            const minutes = zone;
            const query = `${urlBase + profile}/${lng},${lat}`
            + `?contours_minutes=${minutes}&polygons=true&access_token=${process.env.VUE_APP_MAPBOX_TOKEN}`;

            fetch(query)
                .then((response) => response.json())
                .then((data) => {
                    this.map.getSource('iso').setData(data);
                })
                .catch((error) => {
                    console.log(error);
                    // TODO Нормальный вывод алерта или ошибки
                });
        },

        getRoute() {
            const urlBase = 'https://api.mapbox.com/directions/v5/mapbox/';
            // const { lat, lng } = this.Coordinates;
            const profile = 'walking';
            const start = [30.315, 59.939];
            const end = [30.915, 60.139];
            const query = `${urlBase + profile}/${start[0]},${start[1]};${end[0]},${end[1]}?`
            + `steps=true&geometries=geojson&access_token=${process.env.VUE_APP_MAPBOX_TOKEN}`;

            // console.log(query);
            fetch(query)
                .then((response) => response.json())
                .then((data) => {
                    this.map.getSource('route').setData(data);
                })
                .catch((error) => {
                    console.log(error);
                    // TODO Нормальный вывод алерта или ошибки
                });
        },
    },
    mounted() {
        this.mapSize = 'height: 100%; width: 100%;';
        const self = this;
        window.mapboxgl.accessToken = this.accessToken;
        this.map = new window.mapboxgl.Map(this.mapOptions);
        this.map.addControl(new MapboxLanguage({
            defaultLanguage: 'ru',
        }));
        this.map.on('load', () => {
            self.getIsochrone(self.isochroneZone);
            self.mapVisible = true;
            self.$nextTick(() => {
                const mapCanvas = document.getElementsByClassName('mapboxgl-canvas')[0];
                mapCanvas.style.width = '100%';
                mapCanvas.style.height = `${window.innerHeight}px`;
                self.map.resize();
                self.mapLoaded = true;
            });
            self.map.addSource('iso', {
                type: 'geojson',
                data: {
                    type: 'FeatureCollection',
                    features: [],
                },
            });

            self.map.addLayer({
                id: 'isoLayer',
                type: 'fill',
                source: 'iso',
                layout: {},
                paint: {
                    'fill-color': '#5a3fc0',
                    'fill-opacity': 0.3,
                },
            }, 'poi-label');
        });
    },
};

</script>

<style>

</style>
