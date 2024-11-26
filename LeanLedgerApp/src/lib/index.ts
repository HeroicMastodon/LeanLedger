// place files you want to import through the `$lib` alias in this folder.
export function splitPascal(words: string) {
    return words.replace(/([a-z])([A-Z])/g, '$1 $2')
}
