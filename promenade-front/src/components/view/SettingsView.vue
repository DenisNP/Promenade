<template>
    <f7-popup swipe-to-close :opened="settingsOpened" @popup:closed="settingsShow(false)">
        <f7-page>
            <f7-navbar title="Опции" back-link="" @click:back="settingsShow(false)" />
                <f7-list >
                    <f7-list-item  v-for="(item) in User.categories"
                    :title="item.name" :key="item.id" >
                        <font-awesome-icon  slot="media" :icon=item.icon />

                        <f7-toggle
                            slot="after"
                            :checked="item.enabled"
                            @change="settingsCategorySet({ key: item.id })"
                        />
                    </f7-list-item>
                </f7-list>
        </f7-page>
    </f7-popup>
</template>

<script>
import { mapGetters, mapActions } from 'vuex';

export default {
    data: () => ({
        views: 1,
    }),
    computed: {
        ...mapGetters({
            User: 'user',
            settingsOpened: 'settingsOpened',
        }),
    },
    methods: {
        ...mapActions([
            'settingsShow',
            'settingsCategorySet',
        ]),
    },
};
</script>

<style>
</style>
