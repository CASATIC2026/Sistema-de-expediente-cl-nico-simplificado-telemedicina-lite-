<script setup>

import { onMounted } from 'vue'

onMounted(() => {
  const dark = localStorage.getItem('darkMode')

  if (dark === 'true') {
    document.documentElement.classList.add('dark')
  }
})

</script>

<template>
  <router-view />

</template>

<style></style>
