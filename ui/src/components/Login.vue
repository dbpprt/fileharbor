<template>
  <v-layout row justify-center>
    <v-dialog v-model="dialog" persistent width="500px">
      <v-card>
        <v-card-title primary-title>
          <span class="headline" style="width: 100%;">Please login in...</span>
          <div>If you're not registered yet you can sign up for free!</div>
        </v-card-title>
        <v-card-text>
          <v-text-field label="Email" required></v-text-field>
          <v-text-field label="Password" type="password" required></v-text-field>
        </v-card-text>
        <v-card-actions>
          <div class="text-xs-center" style="width: 100%;">
            <v-btn outline class="blue--text darken-3" :router="true" :href="'#/public/register'">Register now</v-btn>
            <v-btn outline class="blue--text darken-3" :router="true" :href="'#/app'">Login</v-btn>
          </div>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-layout>
</template>

<script>
export default {
  name: 'login',
  data () {
    return {
      dialog: true
    }
  }
}
</script>

<style scoped>

</style>
