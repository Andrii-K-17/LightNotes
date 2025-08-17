<template>
  <div class="flex justify-center items-center">
    <svg width="50" height="50" viewBox="0 0 50 50" xmlns="http://www.w3.org/2000/svg">
      <circle
        cx="25"
        cy="25"
        r="15"
        fill="none"
        stroke="#1f47ff"
        stroke-width="4"
        stroke-dasharray="23.55 23.55"
        class="spinner-forward"
      >
      </circle>
      <circle
        cx="25"
        cy="25"
        r="23"
        fill="none"
        stroke="#1f47ff"
        stroke-width="4"
        stroke-dasharray="36.11 36.11"
        class="spinner-reverse"
      >
      </circle>
    </svg>
  </div>
</template>

<style>
  @keyframes spin-f {
    from {
      transform: rotate(0deg);
    }
    to {
      transform: rotate(360deg);
    }
  }

  .spinner-forward {
    animation: spin-f 1s linear infinite;
    transform-origin: 25px 25px; 
  }

  @keyframes spin-r {
    from {
      transform: rotate(360deg);
    }
    to {
      transform: rotate(0deg);
    }
  }

  .spinner-reverse {
    animation: spin-r 1s linear infinite;
    transform-origin: 25px 25px; 
  }
</style>
