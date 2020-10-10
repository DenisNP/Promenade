<template>
        <f7-page>
            <f7-navbar title="Опции" back-link="" @click="close" />
                <f7-list >
                    <f7-list-item
                        v-for="item in user.categories"
                        :title="item.name"
                        :key="item.id"
                    >
                        <font-awesome-icon slot="media" :icon=item.icon style="min-width: 25px;"/>
                        <f7-toggle
                            slot="after"
                            :checked="!!toggles[item.id.toString()]"
                            @change="toggles[item.id.toString()] = $event.target.checked"
                        />
                    </f7-list-item>
                </f7-list>
        </f7-page>
</template>

<script>
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
