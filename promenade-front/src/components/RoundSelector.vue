<template>
    <div class="roundSelector">
        <div class="selectorItem"
             v-for="item in items"
             :key="item.key"
             :class="{active: item.key === range}"
             :style="item.style"
             @click="setActiveItem(item.key)"
        >
            <div class="selectorIcon"></div>
            <div class="selectorText">{{ item.name }}</div>
        </div>
    </div>
</template>

<script>

export default {
    components: {},
    data() {
        return {
            items: [
                {
                    key: 10,
                    name: '10 мин',
                    style: 'margin-left: 60px',
                },
                {
                    key: 15,
                    name: '15 мин',
                    style: 'margin-left: 74px; margin-bottom: 11px;',
                },
                {
                    key: 30,
                    name: '30 мин',
                    style: 'margin-left: 74px; margin-top: 11px;',
                },
                {
                    key: 2,
                    name: 'Рядом',
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
};
</script>

<style>
.roundSelector {
    position: absolute;
    bottom: 60px;
    left: 50%;
    transform: translateY(3px);
}

.selectorItem {
    margin: 7px 0 0;
    display: flex;
    align-items: center;
}

.selectorIcon {
    pointer-events: all;
    height: 18px;
    width: 18px;
    background-color: rgba(0, 49, 104, 0.2);;
    border-radius: 50%;
}

.selectorText {
    font-style: normal;
    font-weight: normal;
    font-size: 10px;
    line-height: 16px;
    text-align: right;
    color: rgba(0, 49, 104, 0.2);
    white-space: pre;
    vertical-align: center;
    margin: 0 0 0 8px;
}

.selectorItem.active .selectorIcon {
    background: #4BB34B;
    box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.5);
}

.selectorItem.active .selectorText {
    color: #4BB34B;
}

</style>
