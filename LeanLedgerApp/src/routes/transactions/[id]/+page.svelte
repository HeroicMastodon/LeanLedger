<script lang="ts">
    import LabeledInput from "$lib/components/LabeledInput.svelte";
    import {defaultTransaction, type Transaction, TransactionTypeOptions} from "$lib/transactions";
    import MoneyInput from "$lib/components/MoneyInput.svelte";
    import PredictiveText from "$lib/components/PredictiveText.svelte";
    import LabeledSelect from "$lib/components/LabeledSelect.svelte";

    const transaction: Transaction = $state(defaultTransaction());
    // TODO: Load transaction
    // TODO: Load Categories
    $effect(() => {
        console.dir($state.snapshot(transaction));
    })
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
    <LabeledSelect
        bind:value={transaction.type}
        label="Type"
        options={TransactionTypeOptions}
    />
    <PredictiveText
        bind:value={transaction.category}
        inputId="category"
        datalistId="categories"
        label="Category"
        options={["Groceries", "Gas"]}
    />
    <LabeledSelect
        bind:value={transaction.sourceAccount}
        label="Source Account"
        optional
        options={[
            {
                display: "Savings",
                value: {
                    id: "savings",
                    name: "Savings"
                }
            },
            {
                display: "Checking",
                value: {
                    id: "checking",
                    name: "Checking"
                }
            },
        ]}
    />
    <LabeledSelect
        bind:value={transaction.destinationAccount}
        label="Source Account"
        optional
        options={[
            {
                display: "Savings",
                value: {
                    id: "savings",
                    name: "Savings"
                }
            },
            {
                display: "Checking",
                value: {
                    id: "checking",
                    name: "Checking"
                }
            },
        ]}
    />
</div>
