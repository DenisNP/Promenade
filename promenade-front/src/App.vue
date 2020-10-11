<template>
    <f7-app :params="f7params">
        <div class="onboarding" v-if="$store.state.showOnboarding">
            <f7-swiper pagination>
                <f7-swiper-slide>
                    <div class="onboarding-slide ob-pic-1">
                        <div class="ob-text">
                            <b>Promenade</b> выдаст вам случайную интересную точку
                            для пешей прогулки рядом с тем местом, где вы находитесь
                        </div>
                        <f7-button class="ob-btn" fill @click="slideNext">Далее</f7-button>
                    </div>
                </f7-swiper-slide>
                <f7-swiper-slide>
                    <div class="onboarding-slide ob-pic-2">
                        <div class="ob-text">
                            Вы сможете выбрать размер зоны для поиска вокруг, а также категории
                            интересующих точек
                        </div>
                        <f7-button class="ob-btn" fill @click="start">
                            Разрешить геолокацию
                        </f7-button>
                    </div>
                </f7-swiper-slide>
            </f7-swiper>
        </div>
        <f7-view :push-state="true" url="/" id="main" main tab tab-active />
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
    methods: {
        slideNext() {
            const sw = this.$f7.swiper.get();
            if (sw) sw.slideNext();
        },
        start() {
            this.$store.dispatch('saveOnboarding');
            this.$store.dispatch('move');
        },
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

.my-text-center {
    text-align: center;
}

.onboarding {
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
    background: white;
    z-index: 5001;
}

.onboarding-slide {
    width: 100vw;
    height: 100vh;
    background: white;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-end;
}

.ob-pic-1 {
    /*background-image: url("");*/
    background-size: cover;
}

.ob-pic-2 {
    /*background-image: url("");*/
    background-size: cover;
}

.ob-text {
    text-align: center;
    margin-bottom: 20px;
    padding: 0 20px;
}

.ob-btn {
    margin-bottom: calc(50px + 1px * env(safe-area-inset-botom, 0));
}
</style>
