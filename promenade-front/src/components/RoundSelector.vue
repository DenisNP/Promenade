<template>
    <div class="roundSelector">
        <div class="selectorItem"
             v-for="item in items"
             :key="item.key"
             :class="{active: item.key === range}"
             :style="item.style"
             @click="setActiveItem(item.key)"
        >
            <div class="selectorIcon"/>
            <div class="selectorText">{{ item.name }}</div>
        </div>
    </div>
</template>

<script>
import VKC from '@denisnp/vkui-connect-helper';
import { ranges } from '@/utils';

export default {
    components: {},
    data() {
        return {
            items: [
                {
                    key: ranges[0],
                    name: '5 мин',
                    style: 'margin-left: 60px',
                },
                {
                    key: ranges[1],
                    name: '10 мин',
                    style: 'margin-left: 74px; margin-bottom: 9px;',
                },
                {
                    key: ranges[2],
                    name: '15 мин',
                    style: 'margin-left: 74px; margin-top: 9px;',
                },
                {
                    key: ranges[3],
                    name: '30 мин',
                    style: 'margin-left: 60px',
                },
            ],
        };
    },
    methods: {
        setActiveItem(key) {
            this.$store.commit('setRange', key);
        },
    },
    computed: {
        range() {
            return this.$store.state.range;
        },
    },
    watch: {
        range() {
            VKC.bridge().send('VKWebAppTapticSelectionChanged', {});
        },
    },
};
</script>

<style>
.roundSelector {
    position: absolute;
    bottom: calc(54px + env(safe-area-inset-bottom));
    left: 50%;
    transform: translateY(3px);
}

.selectorItem {
    margin: 3px 0 0;
    display: flex;
    align-items: center;
    background: transparent;
}

.selectorIcon {
    pointer-events: all;
    height: 23px;
    width: 23px;
    background-color: rgba(0, 49, 104, 0.2);;
    border-radius: 50%;
}

.selectorText {
    font-style: normal;
    font-weight: normal;
    font-size: 12px;
    line-height: 16px;
    text-align: right;
    color: rgba(0, 49, 104, 0.2);
    white-space: pre;
    vertical-align: center;
    margin: 0 0 0 8px;
}

.selectorItem.active .selectorIcon {
    background: #4BB34B;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
}

.selectorItem.active .selectorText {
    color: #4BB34B;
}

</style>
