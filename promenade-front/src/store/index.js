import Vue from 'vue';
import Vuex from 'vuex';
import bridge from '@vkontakte/vk-bridge';
import api from './api';

console.log(api);

Vue.use(Vuex);

export default new Vuex.Store({
    state: {
        user: null,
        poi: null,
        route: null,
        coordinates: {
            lat: 0.0,
            lng: 0.0,
        },
        isNearPoi: false,
        settingsOpened: false,
        range: 10,
    },
    getters: {
        state(state) {
            return state || {};
        },
        poi(state) {
            return state.poi || {};
        },
        user(state) {
            return state.user || {};
        },
        mapState(state) {
            if (!state.poi) return 'isochrone';
            if (!state.isNearPoi) return 'route';
            return 'finish';
        },
        settingsOpened(state) {
            return state.settingsOpened;
        },
        coordinates(state) {
            return state.coordinates || {};
        },
        showIsochrone(state) {
            return (!state.poi && Math.abs(state.coordinates.lat) > 0.0001);
        },
        getCategory(state) {
            return (categoryId) => state.user.categories.find((x) => x.id === categoryId);
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
            state.poi = result.poi;
            state.route = result.route;
            state.coordinates = result.coordinates;
            state.isNearPoi = result.isNearPoi;
        },
        setRange(state, key) {
            state.range = key;
        },
    },
    actions: {
        settingsShow({ commit }, isOpened) {
            commit('settingsShow', isOpened);
        },
        async settingsCategorySet({ commit }, { key }) {
            // TODO добавить проверку если state не изменился иначе будет дергаться
            // подумать
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
                if (e.data.available === 1) {
                    const result = await api('move', e.data);
                    commit('setState', result);
                } else {
                    /* TODO учитывать, если от пользователя пришло available 0 и обрабатывать это
                    возможно коммитить что-то другое */
                }
            });
        },
        async find({ commit, state }) {
            const rangeMapper = {
                2: '0',
                10: '1',
                15: '2',
                30: '3',
            };

            bridge.send('VKWebAppGetGeodata');
            bridge.subscribe(async (e) => {
                if (e.type !== 'VKWebAppGetGeodataResult') return;
                if (e.data.available === 1) {
                    const { lat, long } = e.data;
                    const result = await api('find', {
                        lat,
                        lng: long,
                        rangeId: rangeMapper[state.isochrone.zone],
                    });
                    commit('setState', result);
                } else {
                    /* TODO учитывать, если от пользователя пришло available 0 и обрабатывать это
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
