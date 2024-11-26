<h1 class="h1 mb-8">Accounts</h1>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
    <Accordion>
        {@render accountTable("Bank", true)}
        {@render accountTable("CreditCard", true)}
        {@render accountTable("Merchant", false)}
    </Accordion>
{/await}

{#snippet accountTable(accountType: AccountType, open: boolean)}
    <AccordionItem open={open}>
        {#snippet summary()}
            {splitPascal(accountType)}s
        {/snippet}
        {#snippet content()}
            <div class="table-container">
                <table class="table table-hover table-fixed">
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
                            <td
                                class="text-success-500"
                                class:text-error-400={account.balance < 0}
                            >${account.balance}</td>
                            <td
                                class="text-success-500"
                                class:text-error-400={account.balanceChange < 0}
                            >${account.balanceChange}</td>
                            <td>{account.active}</td>
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
    import {Accordion, AccordionItem, ProgressBar, ProgressRadial} from "@skeletonlabs/skeleton";
    import {splitPascal} from "$lib";
    import {goto} from "$app/navigation";

    type AccountType = "Bank" | "CreditCard" | "Merchant";
    type Account = {
        id: string;
        name: string;
        balance: number;
        balanceChange: number;
        active: boolean;
        lastActivityDate?: string;
    }
    type AccountGrouping = Record<AccountType, Account[]>;
    let accounts: AccountGrouping = $state({
        Bank: [],
        CreditCard: [],
        Merchant: []
    });

    async function load() {
        let response = await apiClient.get<AccountGrouping>("accounts");
        accounts = response.data;
    }

</script>
