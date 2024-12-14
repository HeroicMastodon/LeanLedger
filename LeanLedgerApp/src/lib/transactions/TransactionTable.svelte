<script lang="ts">
    import Money from "$lib/components/Money.svelte";
    import type {Transaction} from "$lib/transactions/index";
    import {dateFromString} from "$lib";

    let {transactions}: { transactions: Transaction[] } = $props();
</script>

<div class="table-container">
    <table class="table">
        <thead>
        <tr>
            <th>Description</th>
            <th>Amount</th>
            <th>Date</th>
            <th>Source Account</th>
            <th>Destination Account</th>
            <th>Category</th>
        </tr>
        </thead>
        <tbody>
        {#each transactions as transaction}
            <tr>
                <td>
                    <a class="text-primary-400" href="/transactions/{transaction.id}">
                        {transaction.description}
                    </a>
                </td>
                <td>
                    <Money amount={transaction.amount} type={transaction.type} />
                </td>
                <td>{dateFromString(transaction.date)}</td>
                <td>
                    {#if transaction.sourceAccount}
                       <a class="text-primary-400" href="/accounts/{transaction.sourceAccount.id}">
                           {transaction.sourceAccount.name}
                       </a>
                    {/if}
                </td>
                <td>
                    {#if transaction.destinationAccount}
                       <a class="text-primary-400" href="/accounts/{transaction.destinationAccount.id}">
                           {transaction.destinationAccount.name}
                       </a>
                    {/if}
                </td>
                <td>{transaction.category ?? ""}</td>
            </tr>
        {/each}
        </tbody>
    </table>
</div>

