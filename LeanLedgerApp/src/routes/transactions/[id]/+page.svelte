<script lang="ts">
    import LabeledInput from "$lib/components/LabeledInput.svelte";
    import {defaultTransaction, type Transaction} from "$lib/transactions";
    import MoneyInput from "$lib/components/MoneyInput.svelte";
    const transaction: Transaction = $state(defaultTransaction());
</script>

<h1 class="h1">Transaction</h1>

<div class="card p-8">
   <LabeledInput
       label="Description"
       type="text"
       bind:value={transaction.description}
   />
    <LabeledInput
        label="Date"
        type="date"
        bind:value={transaction.date}
    />
    <MoneyInput
        label="Amount"
        bind:value={transaction.amount}
    />
<!--TODO: Make SelectInput Component for transaction type-->
<!--  TODO: Make some sort of predictive text input for Category  -->
<!--  TODO: Use the SelectInput Component and predictive stuff together for source and destination accounts  -->
</div>
