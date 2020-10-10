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
body {
    -webkit-user-select: none;
    user-select: none;
    overscroll-behavior-y: none;
}
a, a:active, a:focus{
    outline: none!important;
}
html,
body {
    position: fixed;
    overflow: hidden;
}
:root, :root.theme-dark, :root .theme-dark {
    --f7-theme-color: #3F8AE0;
    --f7-theme-color-rgb: 63, 138, 224;
    --f7-theme-color-shade: #2275d4;
    --f7-theme-color-tint: #629fe6;
    --f7-navbar-height: 52px;
    --f7-list-margin-vertical: 0;
}
</style>
