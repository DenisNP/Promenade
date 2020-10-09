<template>
    <f7-page>
        <div id="mapContainer" ref="mapCont" :style="mapSize" v-show="true" key="mapDiv"></div>
        <MapControls/>
    </f7-page>
</template>

<script>

import { mapGetters, mapActions, mapMutations } from 'vuex';

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
        route() {
            return this.$store.state.route;
        },
        ...mapGetters({
            mapState: 'mapState',
            user: 'user',
            coordinates: 'coordinates',
            poi: 'poi',
            showIsochrone: 'showIsochrone',
        }),
        range() {
            return this.$store.state.range;
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
    },
    methods: {
        ...mapActions([
            'settingsShow',
        ]),
        ...mapMutations({
            mapStateChange: 'mapStateToggle',
        }),
        getIsochrone() {
            const urlBase = 'https://api.mapbox.com/isochrone/v1/mapbox/';
            const lng = 30.315;
            const lat = 59.939;
            // const { lat, lng } = this.Coordinates;
            const profile = 'walking';
            const minutes = this.range;
            const query = `${urlBase + profile}/${lng},${lat}`
            + `?contours_minutes=${minutes}&polygons=true&access_token=${process.env.VUE_APP_MAPBOX_TOKEN}`;

            console.log(query);
            console.log(this.showIsochrone);

            if (this.showIsochrone) {
                fetch(query)
                    .then((response) => response.json())
                    .then((data) => {
                        this.map.getSource('iso').setData(data);
                    })
                    .catch((error) => {
                        console.log(error);
                    // TODO Нормальный вывод алерта или ошибки
                    });
            } else if (this.map.getSource('iso')) { console.log('придумать как обнулять изохрону'); }
        },
        getRoute() {
            if (this.map.getSource('route')) {
                this.map.removeLayer('route');
                this.map.removeSource('route');
            }
            if (!this.mapLoaded || this.route == null) return;

            const coordArray = this.route.points.map((item) => [item.lng, item.lat]);
            console.log(coordArray);

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
                    'line-color': 'blue',
                    'line-width': 4,
                    'line-dasharray': [0, 2],
                },
            });

            const category = this.$store.getters.getCategory(this.poi.categoryId);
            const node = document.createElement('div');
            // node.classList.add('flag');
            node.innerHTML = '<div class="flag">'
            + `<div class="title">${this.poi.description}</div>`
            + '<div class="category">'
            + `<i class="fas fa-${category.icon} icon"></i>`
            + `${category.name}</div></div>`;


            new mapboxgl.Marker(node)
                .setLngLat([this.poi.coordinates.lng, this.poi.coordinates.lat])
                .addTo(this.map);
        },
        drawInitialMap() {
            this.getIsochrone();
            this.getRoute();
            // this.Poi()
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
            self.mapVisible = true;
            self.$nextTick(() => {
                self.map.resize();
                self.mapLoaded = true;
                this.drawInitialMap();
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

.flag .category .icon{
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

</style>
