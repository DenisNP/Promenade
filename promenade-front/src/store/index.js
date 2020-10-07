import Vue from 'vue';
import Vuex from 'vuex';
import bridge from '@vkontakte/vk-bridge';
import api from './api';

console.log(api);

Vue.use(Vuex);

export default new Vuex.Store({
    state: {
        user: {
            id: '463377',
            categories: [
                {
                    enabled: true,
                    id: 1,
                    name: 'архитектура',
                    icon: 'landmark',
                },
                {
                    enabled: true,
                    id: 2,
                    name: 'башни',
                    icon: 'chess-rook',
                },
                {
                    enabled: true,
                    id: 3,
                    name: 'водные объекты',
                    icon: 'faucet',
                },
                {
                    enabled: true,
                    id: 4,
                    name: 'военная история',
                    icon: 'fighter-jet',
                },
                {
                    enabled: false,
                    id: 5,
                    name: 'городские удобства',
                    icon: 'street-view',
                },
                {
                    enabled: true,
                    id: 6,
                    name: 'необычные точки',
                    icon: 'search-location',
                },
                {
                    enabled: false,
                    id: 7,
                    name: 'памятники',
                    icon: 'monument',
                },
                {
                    enabled: true,
                    id: 8,
                    name: 'природные объекты',
                    icon: 'tree',
                },
                {
                    enabled: false,
                    id: 9,
                    name: 'религия',
                    icon: 'mosque',
                },
                {
                    enabled: true,
                    id: 10,
                    name: 'руины',
                    icon: 'house-damage',
                },
            ],
        },
        poi: null,
        route: null,
        coordinates: {
            lat: 0.0,
            lng: 0.0,
        },
        isNearPoi: false,
        settingsOpened: false,
        isochrone: {
            zone: '10',
        },
    },
    getters: {
        state(state) {
            return state;
        },
        poi(state) {
            return state.poi;
        },
        user(state) {
            return state.user;
        },
        mapState(state) {
            if (!state.Poi) return 'isochrone';
            if (!state.isNearPoi) return 'route';
            return 'finish';
        },
        settingsOpened(state) {
            return state.settingsOpened;
        },
        isochrone(state) {
            return state.isochrone;
        },
        coordinates(state) {
            return state.coordinates;
        },
    },
    mutations: {
        settingsShow(state, isOpened) {
            state.settingsOpened = isOpened;
        },
        settingsCategorySet(state, { key, result }) {
            state.user.categories[key].enabled = Boolean(result);
        },
        setState(state, result) {
            state.user = result.user;
            state.roi = result.roi;
            state.route = result.route;
            state.isNearPoi = result.isNearPoi;
        },
        setIsochroneZone(state, key) {
            state.isochrone.zone = key;
        },
    },
    actions: {
        settingsShow({ commit }, isOpened) {
            commit('settingsShow', isOpened);
        },
        async settingsCategorySet({ commit }, { key }) {
            // TODO добавить проверку если state не изменился
            const result = await api('toggle', { categoryId: key });
            commit('setState', result);
        },
        async init({ commit }) {
            const result = await api('init');
            commit('setState', result);
        },
        async move({ commit }) {
            bridge.send('VKWebAppGetGeodata');
            bridge.subscribe(async (e) => {
                if (e.type !== 'VKWebAppGetGeodataResult') return;
                if (e.data.availible === 1) {
                    const result = await api('move', e.data);
                    commit('setState', result);
                } else {
                    /* TODO учитывать, если от пользователя пришло availible 0 и обрабатывать это
                    возможно коммитить что-то другое */
                }
            });
        },
        async find({ commit }, { range }) {
            bridge.send('VKWebAppGetGeodata');
            bridge.subscribe(async (e) => {
                if (e.type !== 'VKWebAppGetGeodataResult') return;
                if (e.data.availible === 1) {
                    const result = await api('find', {
                        geo: e.data,
                        rangeId: range,
                    });
                    commit('setState', result);
                } else {
                    /* TODO учитывать, если от пользователя пришло availible 0 и обрабатывать это
                    возможно коммитить что-то другое */
                }
            });
        },
        async stop({ commit }) {
            const result = await api('stop');
            commit('setState', result);
        },
    },
});
