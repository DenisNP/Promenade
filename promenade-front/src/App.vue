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
    data() {
        return {
            f7params: {
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

        window.addEventListener('popstate', () => {
            if (this.$store.state.currentPoiInfo) {
                this.$store.commit('setCurrentPoiInfo', null);
            }
        });
    },
    computed: {
        geoDisabled() {
            return this.$store.state.geoDisabled;
        },
        geoDenied() {
            return this.$store.state.geoDenied;
        },
        networkDisabled() {
            return this.$store.state.networkDisabled;
        },
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
    watch: {
        geoDisabled(d) {
            if (d) {
                this.$f7.dialog.alert(
                    'Включите геолокацию на телефоне и дайте приложению VK разрешение на получение геопозиции',
                    'Геолокация отключена',
                );
            }
        },
        networkDisabled(d) {
            if (d) {
                const toast = this.$f7.toast.create({
                    text: 'Нет доступа к сети, попробуйте позже.',
                    position: 'center',
                    cssClass: 'my-text-center',
                    closeTimeout: 2000,
                });
                toast.open();
            }
        },
        geoDenied(d) {
            if (d) {
                const toast = this.$f7.toast.create({
                    text: 'Для работы приложения нужен доступ к геопозиции.',
                    position: 'center',
                    cssClass: 'my-text-center',
                    closeTimeout: 2500,
                });
                toast.open();
            }
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
    --f7-navbar-height: 52px!important;
    --f7-list-margin-vertical: 0;
    --f7-sheet-border-color: transparent!important;
    --f7-list-border-color: transparent!important;
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
    background: linear-gradient(0deg, rgba(10,14,23,1) 0%, rgba(29,59,97,1) 100%);
    z-index: 5001;
}

.onboarding-slide {
    width: 100vw;
    max-width: 100vw;
    overflow: hidden;
    height: 100vh;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: flex-end;
}

.ob-pic-1 {
    background-image: url("assets/onb_00000.jpg");
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}

.ob-pic-2 {
    background-image: url("assets/onb_00001.jpg");
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}

.ob-text {
    text-align: center;
    margin-bottom: 20px;
    padding: 0 20px;
    color: white;
}

.ob-btn {
    margin-bottom: 60px;
}

.swiper-pagination-bullet {
    background: white;
}

.swiper-pagination-bullets {
    bottom: 20px!important;
}

.mapboxgl-ctrl {
    /*noinspection CssInvalidFunction*/
    margin-bottom: calc(10px + env(safe-area-inset-bottom) / 2)!important;
}
</style>
