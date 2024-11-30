// place files you want to import through the `$lib` alias in this folder.
export function splitPascal(words: string) {
    return words.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export function formatMoney(amount: number) {
    return amount >= 0 ? `$${amount}` : `-$${amount * -1}`;
}
