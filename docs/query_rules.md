# Search Query Parser Rules

This is ideal/for planning and just a rough idea of the parsing rules. Do not use as guide on the query syntax.

1. `<query> :: <not-flag> <subquery> | <plain-string> | <raw-query> | <filter-rule> <whitespace> <and-condition> <whitespace> "" | <query>`

2. `<whitespace> :: "" | " " <whitespace>`

3. `<plain-string> :: "" | <letter> | <usable-symbol> <plain-string>`

4. `<raw-string> :: <whitespace> <character> | <raw-string>`

5. `<raw-query> :: """ <raw_string> """`

6. `<filter-rule> :: <not-condition> | "" "\" <filename-filter> | <tag-filter> | <path-filter> | <alt-filter>`

7. `<filename-filter> :: "filename" | "file" <filter-condition>`

8. `<tag-filter> :: "tag" <filter-condition>`

9. `<path-filter> :: "path" <filter-condition>`

10. `<filter-condition> :: "" | <not-flag> <equal-condition> | <like-condition>`

11. `<equal-condition> :: "=" <raw-string> | <plain-string>`

12. `<like-condition> :: "~" <raw-string> | <plain-string>`

13. `<and-condition> :: "+" <raw-string> | <plain-string>`

14. `<subquery> :: "(" <query> ")"`

15. `<not-flag> :: "!" | ""`

16. `<alt-filter> :: "alt" | "altname" <filter-condition>`


|  |           | letter | usable-symbol | \ | ( | ) | " | ' ' | = | ~ | + | ! | else |
|--|-----------|--------|---------------|---|---|---|---|-----|---|---|---|---|------|
|1 |query      |    3   |       3       | 4 | 7 | - | 2 |  *  | - | - | 8 | 9 |  -   |
|2 |raw-query  |    2   |       2       | 2 | 2 | 2 | ^ |  2  | 2 | 2 | 2 | 2 |  2   |
|3 |plain      |    3   |       3       | 3 | 3 | 3 | 3 |  ^  | - | - | 8 | 3 |  -   | 
|4 |filter-type|    4   |       4       | - | - | - | - |  -  | 5 | 6 | - | - |  -   |
|5 |rule-exact |    3   |       3       | - | - | - | 2 |  1  | - | - | - | - |  -   |
|6 |rule-like  |    3   |       3       | - | - | - | 2 |  1  | - | - | - | - |  -   |
|7 |subquery   |    3   |       3       | 4 | 7 | ^ | 2 |  *  | - | - | - | 9 |  -   |
|8 |and        |    -   |       -       | - | - | - | - |  8  | - | - | - | 9 |  -   |
|9 |not        |    3   |       3       | 4 | 7 | - | 2 |  -  | 5 | 6 | - | - |  -   |