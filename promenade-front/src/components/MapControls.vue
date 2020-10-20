<template>
    <div class="MapControls">
        <div class="MapControlsGroup" v-if="buttonMode === 'start'">
            <div class="MainButton" @click="tryFind">
                <img src="@/assets/run-person.svg">
            </div>
            <div class="SubButtonLeft"
                @click="$f7router.navigate('/settings')">
                <img src="@/assets/settings.svg">
            </div>
            <RoundSelector/>
        </div>

        <div class="MapControlsGroup" v-if="buttonMode === 'loading'">
            <div class="MainButton">
                <div class="loader-2 center"><span></span></div>
                <div class="loading-phrase">{{phrase}}</div>
            </div>
        </div>

        <div class="MapControlsGroup" v-if="buttonMode === 'clear'">
            <div class="MainButton" @click="stop">
                <img src="@/assets/cross.svg">
            </div>
            <div class="SubButtonLeft" @click="tryFind">
                <img src="@/assets/reload.svg">
            </div>
        </div>

        <div class="MapControlsGroup" v-if="buttonMode === 'finish'">
            <div class="MainButton" @click="finish">
                <img src="@/assets/check.svg">
            </div>
            <div class="SubButtonLeft" @click="goToStories">
                <img src="@/assets/story.svg">
            </div>
        </div>
    </div>
</template>

<script>
import VKC from '@denisnp/vkui-connect-helper';
import { mapActions } from 'vuex';
import RoundSelector from './RoundSelector.vue';
import loadingPhrases from '../loadingPhrases';
import { shuffle } from '../utils';

export default {
    components: {
        RoundSelector,
    },
    data() {
        return {
            interval: 0,
            phrase: '',
        };
    },
    computed: {
        isLoading() {
            return this.$store.state.isLoading;
        },
        buttonMode() {
            if (this.isLoading) {
                return 'loading';
            }
            if (!this.$store.state.poi) {
                return 'start';
            }
            if (!this.$store.state.isNearPoi) {
                return 'clear';
            }
            return 'finish';
        },
    },
    watch: {
        isLoading(l) {
            clearInterval(this.interval);
            if (l) {
                this.interval = setInterval(this.nextPhrase, 2500);
                shuffle(loadingPhrases);
                this.nextPhrase();
            } else {
                this.phrase = '';
            }
        },
    },
    methods: {
        ...mapActions([
            'find',
            'stop',
        ]),
        async tryFind() {
            const allowed = await this.find();
            if (!allowed) return;
            this.$nextTick(() => {
                if (
                    !this.$store.state.poi
                    && !this.$store.state.geoDenied
                    && !this.$store.state.geoDisabled
                    && !this.$store.state.networkDisabled
                ) {
                    const toast = this.$f7.toast.create({
                        text: 'Ничего не найдено. Попробуйте выбрать больше категорий или расширить зону.',
                        position: 'center',
                        cssClass: 'my-text-center',
                        closeTimeout: 3500,
                    });
                    toast.open();
                    VKC.bridge().send('VKWebAppTapticNotificationOccurred', { type: 'error' });
                } else {
                    VKC.bridge().send('VKWebAppTapticNotificationOccurred', { type: 'success' });
                }
            });
        },
        async finish() {
            const wasNear = this.$store.state.isNearPoi;
            await this.stop();
            if (wasNear) {
                const toast = this.$f7.toast.create({
                    text: 'Точка была сохранена, как посещённая.',
                    position: 'center',
                    cssClass: 'my-text-center',
                    closeTimeout: 2000,
                });
                toast.open();
            }
        },
        goToStories() {
            VKC.send('VKWebAppShowStoryBox', { background_type: 'none' });
        },
        nextPhrase() {
            const p = loadingPhrases.shift();
            this.phrase = p;
            loadingPhrases.push(p);
        },
    },
};

</script>

<style>

.MapControls {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    pointer-events: none;
    z-index: 6;
    transform: translate3d(0,0,0);
    overflow: visible;
}
.MapControls * {
    pointer-events: all;
}

.MainButton {
    position: absolute;
    /*noinspection CssInvalidFunction*/
    bottom: calc(50px + env(safe-area-inset-bottom));
    left: 50%;
    margin: 0 -50px;
    pointer-events: all;
    height: 110px;
    width: 110px;
    background-color: #ffffff;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.25);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 4;
}

.SubButtonLeft {
    position: absolute;
    /*noinspection CssInvalidFunction*/
    bottom: calc(50px + env(safe-area-inset-bottom));
    left: 50%;
    margin: 0 0 0 -65px;
    pointer-events: all;
    height: 41px;
    width: 41px;
    background-color: #3F8AE0; /* var(--button_primary_background) */
    border-radius: 50%;
    display: flex;
    align-items: center; /*высота*/
    justify-content: center; /*ширина*/
    box-shadow: 0 4px 4px rgba(0, 0, 0, 0.25);
    z-index: 5;
    overflow: visible;
}

.loading-phrase {
    position: absolute;
    display: flex;
    justify-content: center;
    align-items: center;
    text-align: center;
    font-size: 11px;
    width: 85px;
    max-width: 85px;
}
</style>
