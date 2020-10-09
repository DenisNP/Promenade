<template>
    <div class="MapControls">
        <div class="MapControlsGroup" v-if="mapState === 'isochrone'">
            <div class="MainButton" @click="find()">
                <img src="@/assets/run-person.svg">
            </div>
            <div class="SubButtonLeft"
                @click="$f7router.navigate('/settings')">
                <img src="@/assets/settings.svg">
            </div>
            <RoundSelector/>
        </div>

        <div class="MapControlsGroup" v-if="mapState === 'route' ">
            <div class="MainButton" @click="stop()">
                <img src="@/assets/cross.svg">
            </div>
            <div class="SubButtonLeft" @click="find()">
                <img src="@/assets/reload.svg">
            </div>
        </div>

        <div class="MapControlsGroup" v-if="mapState === 'finish'">
            <div class="MainButton" @click="stop()">
                <img src="@/assets/check.svg">
            </div>
            <div class="SubButtonLeft" @click="goToStories()">
                <img src="@/assets/story.svg">
            </div>
        </div>
    </div>
</template>

<script>

import { mapGetters, mapActions } from 'vuex';

import RoundSelector from './RoundSelector.vue';

export default {
    components: {
        RoundSelector,
    },
    data: () => ({}),
    computed: {
        ...mapGetters({
            mapState: 'mapState',
            isochrone: 'isochrone',
        }),
    },
    methods: {
        ...mapActions([
            'settingsShow',
            'find',
            'stop',
        ]),
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
}
.MapControls * {
    pointer-events: all;
}

.MainButton {
position: absolute;
    bottom: 50px;
    left: 50%;
    margin: 0 -50px;
    pointer-events: all;
    height: 110px;
    width: 110px;
    background-color: #ffffff;
    box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.25);
    border-radius: 50%;
    display: flex;
    align-items: center; /*высота*/
    justify-content: center; /*ширина*/
    z-index: 1;
}

.SubButtonLeft {
position: absolute;
    bottom: 50px;
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
    filter: drop-shadow(0px 4px 4px rgba(0, 0, 0, 0.25));
    z-index: 2;
}

</style>
