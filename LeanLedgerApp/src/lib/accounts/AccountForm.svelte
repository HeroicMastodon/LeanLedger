<script lang="ts">
import {accountTypes} from "$lib/accounts/index.js";
import {splitPascal} from "$lib";
import {faDollar} from "@fortawesome/free-solid-svg-icons";
import {Fa} from "svelte-fa";
import {type AccountData} from "$lib/accounts";

let {account = $bindable()}: {account: AccountData} = $props();
</script>

<div class="card p-4 grid grid-cols-1 md:grid-cols-6 gap-4 max-w-3xl items-center">
    <label class="label md:col-span-3">
        <span>Name</span>
        <input class="input" bind:value={account.name} placeholder="America First" />
    </label>
    <label class="label md:col-span-3">
        <span>Account Type</span>
        <select class="select" bind:value={account.accountType}>
            {#each accountTypes as accountType}
                <option value={ accountType }>{splitPascal(accountType)}</option>
            {/each}
        </select>
    </label>
    <div class="label md:col-span-2">
        <span>Opening Balance</span>
        <div class="input-group input-group-divider grid-cols-[auto_1fr_auto]">
            <div class="input-group-shim">
                <Fa icon={faDollar}></Fa>
            </div>
            <input type="number" bind:value={account.openingBalance} />
        </div>
    </div>
    <label class="label md:col-span-2">
        <span>Opening Date</span>
        <input type="date" class="input" bind:value={account.openingDate} />
    </label>
    <div class="md:col-span-2 flex flex-col">
        <label class="md:col-span-1 flex space-x-2 items-center">
            <input type="checkbox" class="checkbox mt-2 mb-2" bind:checked={account.includeInNetWorth} />
            <span>Part of Net Worth</span>
        </label>
        <label class="md:col-span-1 flex space-x-2 items-center">
            <input type="checkbox" class="checkbox mt-2 mb-2" bind:checked={account.active} />
            <span>Active</span>
        </label>
    </div>
    <label class="label md:col-span-6">
        <span>Notes</span>
        <textarea class="textarea" bind:value={account.notes}></textarea>
    </label>
</div>
