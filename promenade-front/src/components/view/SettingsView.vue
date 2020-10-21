<template>
        <f7-page>
            <f7-navbar title="Категории" back-link="" @click="close" />
                <f7-list >
                    <f7-list-item
                        v-for="item in user.categories"
                        :title="firstUpperCase(item.name)"
                        :key="item.id"
                    >
                        <font-awesome-icon slot="media" :icon=item.icon style="min-width: 25px;"/>
                        <f7-toggle
                            slot="after"
                            :checked="!!toggles[item.id.toString()]"
                            @change="setCategory(item.id.toString(), $event.target)"
                        />
                    </f7-list-item>
                </f7-list>
        </f7-page>
</template>

<script>
import { firstUpperCase } from '@/utils';

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
</style>
