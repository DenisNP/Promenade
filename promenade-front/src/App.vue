<template>
    <f7-app :params="f7params">
            <f7-view :push-state="true" url="/" id="main" main tab tab-active />
            <!--<f7-view :push-state="true" url="/settings" id="settings" /> -->
    </f7-app>
</template>

<script>

import MapView from './components/view/MapView.vue';
import SettingsView from './components/view/SettingsView.vue';

export default {
    components: {
    },
    data() {
        return {
            f7params: {
                theme: 'ios',
                name: 'Название',
                id: 'id',
                routes: [
                    {
                        path: '/',
                        component: MapView,
                    },
                    {
                        path: '/settings',
                        component: SettingsView,
                    },
                ],
            },
            moveInterval: null,
        };
    },
    mounted() {
        this.$store.dispatch('start');
    },
};

</script>

<style>
</style>
