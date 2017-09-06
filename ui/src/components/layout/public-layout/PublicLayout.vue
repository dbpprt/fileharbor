<template>
  <v-app class="bg-container">
    <div v-bind:class="classObject"></div>
    <div class="auroral-stars"></div>
    <main>
      <v-container fluid>
        <router-view />
      </v-container>
    </main>
  </v-app>
</template>

<script>
export default {
  name: 'public-layout',
  computed: {
    classObject: function () {
      const classes = ['auroral-northern', 'auroral-northern-dusk', 'auroral-northern-intense', 'auroral-northern-warm']
      return [classes[Math.floor(Math.random() * classes.length)]]
    }
  }
}
</script>
 
<style src="./auroral.css" scoped></style>

<style>
.bg-container {
  display: block;
  position: relative;
  overflow: hidden;
}
</style>
