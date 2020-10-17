import Vue from 'vue';
import Vuex from 'vuex';
import VKC from '@denisnp/vkui-connect-helper';
import {
    geoDistance, getAppId, getPlatform, ranges,
} from '../utils';
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
        range: 15,
        isLoading: false,
        userName: 'Я',
        showOnboarding: false,
        geoDisabled: false,
        geoDenied: false,
        networkDisabled: false,
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
        setCoordinates(state, coordinates) {
            state.coordinates = coordinates;
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
        setShowOnboarding(state, onb) {
            state.showOnboarding = onb;
        },
        setGeoDisabled(state, d) {
            state.geoDisabled = d;
        },
        setGeoDenied(state, d) {
            state.geoDenied = d;
        },
        setNetworkDisabled(state, d) {
            state.networkDisabled = d;
        },
    },
    actions: {
        async start({ state, commit, dispatch }) {
            VKC.init({
                appId: getAppId(),
                accessToken: getPlatform() === 'local' ? process.env.VUE_APP_VK_DEV_TOKEN : '',
                asyncStyle: true,
                apiVersion: '5.120',
            });

            // set bar color
            VKC.bridge().send(
                'VKWebAppSetViewSettings',
                { status_bar_style: 'dark', action_bar_color: '#e7eff9' },
            );

            // onboarded
            const [onb] = await VKC.send(
                'VKWebAppStorageGet',
                { keys: ['onboarded'] },
            );
            if (onb && onb.keys) {
                if (!onb.keys.some((k) => k.key === 'onboarded' && k.value)) {
                    commit('setShowOnboarding', true);
                }
            }

            await dispatch('init');

            if (state.user && !state.showOnboarding) dispatch('move');

            // store user name
            const [userData] = await VKC.send('VKWebAppGetUserInfo');
            if (userData) {
                let userName = `${userData.first_name || ''} ${userData.last_name || ''}`;
                userName = userName.trim();
                if (userName) commit('setUserName', userName);
            }
        },
        async api({ commit }, { method, data }) {
            commit('setNetworkDisabled', false);
            const result = await api(method, data || {});
            if (!result) {
                commit('setNetworkDisabled', true);
                return null;
            }
            return result;
        },
        async saveSettings({ commit, dispatch }, data) {
            const result = await dispatch('api', { method: 'settings', data });
            commit('setState', result);
        },
        async init({ commit, dispatch }) {
            const result = await dispatch('api', { method: 'init' });
            commit('setState', result);
        },
        async move({ state, commit, dispatch }) {
            const geo = await dispatch('getGeo');
            if (!geo) return;
            const dist = geoDistance(
                [geo.lat, geo.long],
                [state.coordinates.lat, state.coordinates.lng],
            );
            if (dist < 0.001) return;
            const result = await dispatch('api', { method: 'move', data: { lat: geo.lat, lng: geo.long } });
            commit('setState', result);
        },
        async find({ commit, state, dispatch }) {
            const rangeId = ranges.findIndex((r) => r === state.range);
            const geo = await dispatch('getGeo');
            if (!geo) return false;
            commit('setIsLoading', true);
            const result = await dispatch('api', {
                method: 'find',
                data: {
                    lat: geo.lat,
                    lng: geo.long,
                    rangeId,
                },
            });
            commit('setIsLoading', false);
            commit('setState', result);
            return true;
        },
        async getGeo({ commit }) {
            commit('setGeoDenied', false);
            commit('setGeoDisabled', false);
            const [geo] = await VKC.send('VKWebAppGetGeodata');
            if (!geo) {
                commit('setCoordinates', { lat: 0, lng: 0 });
                commit('setGeoDenied', true);
                return null;
            }
            if (geo && !geo.available) {
                commit('setCoordinates', { lat: 0, lng: 0 });
                commit('setGeoDisabled', true);
                return null;
            }
            return geo;
        },
        async stop({ commit, dispatch }) {
            commit('setIsLoading', true);
            const result = await dispatch('api', { method: 'stop' });
            commit('setState', result);
            commit('setIsLoading', false);
        },
        saveOnboarding({ commit }) {
            commit('setShowOnboarding', false);
            VKC.send('VKWebAppStorageSet', { key: 'onboarded', value: '1' });
        },
    },
});
