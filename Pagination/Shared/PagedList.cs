namespace API.Pagination.Shared
{
    public class PagedList<T> : List<T> where T : class
    {
        // Lista paginada. Com entidades e atributos usados na paginação
        // a herança de List<T> é usada para guardar os itens (companies, employees ou pruducts)
        public int CurrentPage { get; set; } // número da página atual
        public int TotalPages { get; set; } // páginas necessárias para colocar todos os itens
        public int PageSize { get; set; } // quantidade de itens na página
        public int TotalCount { get; set; } // contagem total dos itens

        public bool HasPrevious => CurrentPage > 1; // true se tem alguma página antes da atual
        public bool HasNext => CurrentPage < TotalPages; // true se é necessário mais alguma página

        public PagedList(List<T> items, int totalCount, int currentPage, int pageSize)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int) Math.Ceiling(TotalCount / (double) PageSize);

            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int currentPage, int pageSize)
        {
            // source contém todos as entidades no banco de dados
            int count = source.Count(); // número de dados no banco
            List<T> items = source.Skip((currentPage - 1) * pageSize)
                .Take(pageSize).ToList(); // pulando com base no número da página e pegando só o necessário
                                          // com base no tamanho da página

            return new PagedList<T>(items, count, currentPage, pageSize);
        }

    }
}
