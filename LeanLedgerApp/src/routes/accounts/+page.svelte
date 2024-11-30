<h1 class="h1 mb-8">Accounts</h1>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then accounts}
    <Accordion>
        {@render accountTable("Bank", true, accounts)}
        {@render accountTable("CreditCard", true, accounts)}
        {@render accountTable("Merchant", false, accounts)}
    </Accordion>
{/await}

{#snippet accountTable(accountType: AccountType, open: boolean, accounts: AccountGrouping)}
    <AccordionItem open={open}>
        {#snippet summary()}
            {splitPascal(accountType)}s
        {/snippet}
        {#snippet content()}
            <div class="table-container">
                <table class="table table-hover md:table-fixed">
                    <thead>
                    <tr>
                        <th>Name</th>
                        <th>Balance</th>
                        <th>Change</th>
                        <th>Active</th>
                        <th>Last Activity</th>
                    </tr>
                    </thead>
                    <tbody>
                    {#each accounts[accountType] as account}
                        <tr class="hover:cursor-pointer" onclick={() => goto(`/accounts/${account.id}`)}>
                            <td>
                                {account.name}
                            </td>
                            <td>
                                <Money amount={account.balance} />
                            <td
                                class="text-success-500"
                                class:text-error-400={account.balanceChange < 0}
                            >${account.balanceChange}</td>
                            <td>
                                <input type="checkbox" class="checkbox" checked={account.active} disabled />
                            </td>
                            <td>{account.lastActivityDate ?? "No Activity"}</td>
                        </tr>
                    {/each}
                    </tbody>
                </table>
            </div>
        {/snippet}
    </AccordionItem>

{/snippet}

<script lang="ts">
    import {apiClient} from "$lib/apiClient";
    import {Accordion, AccordionItem, ProgressBar} from "@skeletonlabs/skeleton";
    import {splitPascal} from "$lib";
    import type {AccountGrouping, AccountType,} from "$lib/accounts";
    import {goto} from "$app/navigation";
    import Money from "$lib/components/Money.svelte";

    async function load() {
        let response = await apiClient.get<AccountGrouping>("accounts");
        return response.data;
    }

</script>
