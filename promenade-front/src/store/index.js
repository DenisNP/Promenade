import Vue from 'vue';
import Vuex from 'vuex';
import VKC from '@denisnp/vkui-connect-helper';
import {
    getAppId, getPlatform, ranges,
} from '@/store/utils';
import api from './api';


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
        isLoading: false,
        userName: 'Я',
    },
    getters: {
        hasCoordinates(state) {
            return Math.abs(state.coordinates.lat) > 0.0001;
        },
        showIsochrone(state, getters) {
            return !state.poi && getters.hasCoordinates;
        },
        getCategory(state) {
            return (categoryId) => state.user.categories.find((x) => x.id === categoryId);
        },
    },
    mutations: {
        setState(state, result) {
            if (!result) return;
            state.user = result.user;
            state.poi = result.poi;
            state.route = result.route;
            state.coordinates = result.coordinates;
            state.isNearPoi = result.isNearPoi;
        },
        setRange(state, key) {
            state.range = key;
        },
        setIsLoading(state, isLoading) {
            state.isLoading = isLoading;
        },
        setUserName(state, uName) {
            state.userName = uName;
        },
    },
    actions: {
        async start({ commit, dispatch }) {
            VKC.init({
                appId: getAppId(),
                accessToken: getPlatform() === 'local' ? process.env.VUE_APP_VK_DEV_TOKEN : '',
                asyncStyle: true,
                apiVersion: '5.120',
            });

            // set bar color
            VKC.bridge().send(
                'VKWebAppSetViewSettings',
                { status_bar_style: 'dark', action_bar_color: '#5a3fc0' },
            );

            dispatch('init');

            // store user name
            const [userData] = await VKC.send('VKWebAppGetUserInfo');
            if (userData) {
                let userName = `${userData.first_name || ''} ${userData.last_name || ''}`;
                userName = userName.trim();
                if (userName) commit('setUserName', userName);
            }
        },
        async saveSettings({ commit }, data) {
            const result = await api('settings', data);
            commit('setState', result);
        },
        async init({ commit }) {
            const result = await api('init');
            commit('setState', result);
        },
        async move({ commit }) {
            const [geo] = await VKC.send('VKWebAppGetGeodata');
            if (!geo) return;
            const result = await api('move', { lat: geo.lat, lng: geo.long });
            commit('setState', result);
        },
        async find({ commit, state }) {
            const rangeId = ranges.findIndex((r) => r === state.range);
            const [geo] = await VKC.send('VKWebAppGetGeodata');
            if (!geo) return;
            commit('setIsLoading', true);
            const result = await api('find', {
                lat: geo.lat,
                lng: geo.long,
                rangeId,
            });
            commit('setIsLoading', false);
            commit('setState', result);
        },
        async stop({ commit }) {
            const result = await api('stop');
            commit('setState', result);
        },
    },
});
