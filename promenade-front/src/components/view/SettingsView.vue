<template>
        <f7-page>
            <f7-navbar title="Опции" back-link="" @click="close"/>
            <f7-toolbar tabbar position="top">
                <f7-link :tab-link-active="tab === 1" @click="setTab(1)">Категории</f7-link>
                <f7-link :tab-link-active="tab === 2" @click="setTab(2)">Достижения</f7-link>
            </f7-toolbar>
            <f7-list v-show="tab === 1">
                <f7-list-item
                    v-for="item in user.categories"
                    :title="firstUpperCase(item.name)"
                    :key="item.id"
                >
                    <i
                        slot="media"
                        :class="`fas fa-${item.icon} icon`"
                        style="min-width: 25px; text-align: center;"
                    />
                    <f7-toggle
                        slot="after"
                        :checked="!!toggles[item.id.toString()]"
                        @change="setCategory(item.id.toString(), $event.target)"
                    />
                </f7-list-item>
            </f7-list>
            <f7-list media-list v-show="tab === 2">
                <f7-list-item
                    v-for="a in achievements"
                    :key="a.name"
                    :text="a.description"
                    class="achievement-item"
                    :class="{'not-active': a.progress <= 0}"
                >
                    <div slot="title" style="font-weight: 500;">{{a.name}}</div>
                    <i
                        slot="media"
                       :class="`fas fa-${a.icon}
                            icon achievement-icon-sub${a.progress >= 100 ? ' full-done' : ''}`"
                    />
                    <f7-progressbar
                        class="a-progress-bar"
                        :class="{'pb-done': a.progress >= 100}"
                        :progress="a.progress"
                    />
                </f7-list-item>
            </f7-list>
        </f7-page>
</template>

<script>
import { firstUpperCase, getNumericPhrase } from '@/utils';

export default {
    data() {
        return {
            toggles: {},
        };
    },
    computed: {
        user() {
            return this.$store.state.user;
        },
        achievements() {
            return this.$store.state.achievements
                .map((a) => ({
                    ...a,
                    description: this.getDescription(a),
                    progress: Math.min(100, (a.done * 100) / a.count),
                }));
        },
        tab() {
            return this.$store.state.settingsTab;
        },
    },
    methods: {
        close() {
            this.$f7router.back();
        },
        setCategory(id, t) {
            this.toggles[id] = t.checked;
            if (Object.values(this.toggles).filter((x) => x).length === 0) {
                this.$nextTick(() => {
                    this.toggles[id] = true;
                    // eslint-disable-next-line no-param-reassign
                    t.checked = true;
                });
                const toast = this.$f7.toast.create({
                    text: 'Нужно выбрать хотя бы одну категорию.',
                    position: 'center',
                    cssClass: 'my-text-center',
                    closeTimeout: 1500,
                });
                toast.open();
            }
        },
        firstUpperCase(s) {
            return firstUpperCase(s);
        },
        getDescription(a) {
            const category = this.$store.getters.getCategory(a.categoryId);
            return `Посетить ${a.count} ${getNumericPhrase(a.count, 'точку', 'точки', 'точек')}
                в категории «${firstUpperCase(category.name)}»`;
        },
        setTab(t) {
            this.$store.commit('setSettingsTab', t);
        },
    },
    beforeMount() {
        const { categories } = this.$store.state.user;
        categories.forEach((c) => {
            this.toggles[c.id.toString()] = c.enabled;
        });
    },
    beforeDestroy() {
        const categoriesData = [];
        Object.entries(this.toggles).forEach(([id, enabled]) => {
            categoriesData.push({
                id,
                enabled,
            });
        });

        this.$store.dispatch('saveSettings', { categories: categoriesData });
    },
};
</script>

<style>
.achievement-item {
    justify-self: center;
}

.achievement-item .item-media {
    align-self: center!important;
}

.achievement-icon-sub {
    min-width: 40px;
    font-size: 30px;
    text-align: center;
    color: var(--f7-theme-color);
}

.a-progress-bar {
    margin-top: 10px;
}

.pb-done {
    --f7-progressbar-progress-color: #4BB34B!important;
}

.not-active {
    opacity: 0.6;
}

.not-active .achievement-icon-sub {
    color: #818C99!important;
}

.full-done {
    color: #4BB34B!important;
}
</style>
