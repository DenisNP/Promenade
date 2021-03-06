import Vue from 'vue';

import Framework7 from 'framework7/framework7-lite.esm.bundle';
import Framework7Vue from 'framework7-vue/framework7-vue.esm.bundle';

import 'framework7/css/framework7.bundle.css';
import 'framework7-icons';

import '@fortawesome/fontawesome-free/css/all.css';

import './components/loader.css';

import App from './App.vue';
import store from './store';

Framework7.use(Framework7Vue);

window.mapboxgl = require('mapbox-gl');

Vue.config.productionTip = false;

new Vue({
    store,
    render: (h) => h(App),
}).$mount('#app');
