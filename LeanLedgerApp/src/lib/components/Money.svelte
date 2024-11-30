<script lang="ts">
    import {formatMoney} from "$lib";

    const {amount}: { amount: number } = $props();

</script>

<span
    class:text-success-500={amount >= 0}
    class:text-error-400={amount < 0}
>
    {formatMoney(amount)}
</span>
