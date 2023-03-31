# Search Query Parser Rules

1. `<query> :: <subquery> | <plain-string> | <raw-query> | <filter-rule> <whitespace> "" | <query>`

2. `<whitespace> :: "" | " " <whitespace>`

3. `<plain-string> :: "" | <letter> | <usable-symbol> <plain-string>`

4. `<raw-string> :: <whitespace> <character> | <raw-string>`

5. `<raw-query> :: """ <raw_string> """`

6. `<filter-rule> :: "\" <filename-filter> | <tag-filter> | <path-filter>`

7. `<filename-filter> :: "filename" <filter-condition>`

8. `<tag-filter> :: "tag" <filter-condition>`

9. `<path-filter> :: "path" <filter-condition>`

10. `<filter-condition> :: <equal-condition> | <like-condition>`

11. `<equal-condition> :: "=" <raw-string> | <plain-string>`

12. `<like-condition> :: "~" <raw-string> | <plain-string>`

13. `<subquery> :: "(" <query> ")"`


|  |           | letter | usable-symbol | \ | ( | ) | " | ' ' | = | ~ | else |
|--|-----------|--------|---------------|---|---|---|---|-----|---|---|------|
|1 |query      |    3   |       3       | 4 | 7 | - | 2 |  *  | - | - |  -   |
|2 |raw-query  |    2   |       2       | 2 | 2 | 2 | ^ |  2  | 2 | 2 |  2   |
|3 |plain      |    3   |       3       | 3 | - | - | 3 |  1  | - | - |  -   | 
|4 |filter-type|    4   |       4       | - | - | - | - |  -  | 5 | 6 |  -   |
|5 |rule-exact |    3   |       3       | - | - | - | 2 |  1  | - | - |  -   |
|6 |rule-like  |    3   |       3       | - | - | - | 2 |  1  | - | - |  -   |
|7 |subquery   |    3   |       3       | 4 | 7 | ^ | 2 |  *  | - | - |  -   |