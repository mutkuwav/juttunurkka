
//Copyright 2026 Matias Meriläinen

//This file is part of "Juttunurkka".

//Juttunurkka is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, version 3 of the License.

//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.

namespace Prototype
{
    public static class ApiConfig
    {
        // baseurl tulee vaihtaa jos testaa local backendia nopeampia muutoksia varten
        //local hosti 10.0.2.2:8080
        //2026 projektin server ip on 86.50.20.47:8080, serveri on voimassa 31.7.2026 saakka
        public static string BaseUrl = "http://86.50.20.47:8080/";
    }
}