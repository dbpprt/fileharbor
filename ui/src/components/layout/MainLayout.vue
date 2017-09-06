<template>
  <v-app toolbar>
    <v-navigation-drawer absolute persistent light enable-resize-watcher :mini-variant.sync="mini" v-model="drawer" overflow>
      <v-toolbar flat class="transparent">
        <v-list class="pa-0">
          <v-list-tile avatar>
            <v-list-tile-avatar>
              <img src="https://randomuser.me/api/portraits/men/85.jpg" />
            </v-list-tile-avatar>
            <v-list-tile-content>
              <v-list-tile-title>John Leider</v-list-tile-title>
            </v-list-tile-content>
            <v-list-tile-action>
              <v-btn icon @click.native.stop="mini = !mini">
                <v-icon>chevron_left</v-icon>
              </v-btn>
            </v-list-tile-action>
          </v-list-tile>
        </v-list>
      </v-toolbar>
      <v-list class="pt-0" dense>
        <v-divider></v-divider>
        <v-list-tile v-for="item in items" :key="item.title" :router="item.router" :href="item.href">
          <v-list-tile-action>
            <v-icon>{{ item.icon }}</v-icon>
          </v-list-tile-action>
          <v-list-tile-content>
            <v-list-tile-title>{{ item.title }}</v-list-tile-title>
          </v-list-tile-content>
        </v-list-tile>
      </v-list>
    </v-navigation-drawer>
    <v-toolbar class="blue darken-3" dark>
      <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
      <v-toolbar-title>Fileharbor</v-toolbar-title>
    </v-toolbar>
    <main>
      <v-container fluid>
        <transition name="fade" mode="out-in">
          <router-view class="view"></router-view>
        </transition>
      </v-container>
    </main>
  </v-app>
</template>

<script>
export default {
  name: 'main-layout',
  data () {
    return {
      drawer: true,
      items: [
        { title: 'Home', icon: 'dashboard', router: true, href: '#/' },
        { title: 'About', icon: 'question_answer', router: true, href: '#/app/about' }
      ],
      mini: false,
      right: null
    }
  }
}
</script>
 