<div class="mb-8 flex gap-4 items-end">
    <h1 class="h1">Accounts</h1>
    <button onclick={() => accountDialog?.showModal()} class="btn variant-filled-primary">New Account</button>
    <DefaultDialog bind:dialog={accountDialog}>
        <div class="flex flex-col gap-4">
            <h2 class="h2"> New Account </h2>
            <AccountForm bind:account={newAccount} />
            <div class="flex gap-4 items-center justify-around">
                <button onclick={() => accountDialog?.close()} class="btn variant-outline-error">Cancel</button>
                <button onclick={saveNewAccount} class="btn variant-filled-success">Create</button>
            </div>
        </div>
    </DefaultDialog>
</div>
{#await load()}
    <ProgressBar meter="bg-primary-500" track="bg-primary-500/30" />
{:then _}
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
    import {type AccountData, type AccountGrouping, type AccountType, defaultAccountData,} from "$lib/accounts";
    import {goto} from "$app/navigation";
    import Money from "$lib/components/Money.svelte";
    import AccountForm from "$lib/accounts/AccountForm.svelte";
    import DefaultDialog from "$lib/components/DefaultDialog.svelte";

    let accounts: AccountGrouping = $state({
        Bank: [],
        CreditCard: [],
        Merchant: []
    });

    async function load() {
        let response = await apiClient.get<AccountGrouping>("accounts");
        accounts = response.data;
    }

    let accountDialog: HTMLDialogElement | undefined = $state();
    let newAccount: AccountData = $state(defaultAccountData());

    async function saveNewAccount() {
        const response = await apiClient.post<AccountData>("accounts", newAccount);
        await load();
        newAccount = defaultAccountData();
        accountDialog?.close();
    }
</script>
