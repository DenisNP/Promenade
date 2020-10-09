<template>
    <f7-app :params="f7params">
            <f7-view :push-state="true" url="/" id="main" main tab tab-active />
            <!--<f7-view :push-state="true" url="/settings" id="settings" /> -->
    </f7-app>
</template>

<script>

import bridge from '@vkontakte/vk-bridge';
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
        bridge.send('VKWebAppInit');
        this.$store.dispatch('init');

        if (this.moveInterval) clearInterval(this.moveInterval);
        this.moveInterval = setInterval(() => {
            this.$store.dispatch('move');
        }, 5000); // 5 секнуд
    },
};

</script>

<style>
</style>
