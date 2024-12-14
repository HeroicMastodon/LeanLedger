<script lang="ts">
    import {formatMoney} from "$lib";
    import type {TransactionType} from "$lib/transactions";

    const {amount, type}: { amount: number; type?: TransactionType } = $props();

</script>

{#if type}
<span
    class:text-success-500={type === "Income"}
    class:text-error-400={type === "Expense"}
    class:text-warning-400={type === "Transfer"}
>
    {formatMoney(amount, type)}
</span>
{:else}
<span
    class:text-success-500={amount > 0}
    class:text-error-400={amount < 0}
    class:text-warning-400={amount === 0}
>
    {formatMoney(amount)}
</span>
{/if}

