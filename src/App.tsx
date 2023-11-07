import { Outlet, useLocation, useNavigate } from 'react-router-dom'

import Header from './components/Header'
import { BsGithub, BsInstagram, BsLinkedin } from 'react-icons/bs';
import { createContext, useEffect, useState } from 'react';
import { getDataAccount } from './shared/services/AuthService';
import Alert, { AlertProps } from './components/Alert';

/* type TypeAlerContext = (alertProps: AlertProps) => void */
export const MainContext = createContext<any>({});

function App() {

  const pathname = useLocation();
  const navigate = useNavigate();
  const [alert, setAlert] = useState<AlertProps>({
    text: "",
    status: "info",
    isVisible: false,
    closed: () => setAlert({ ...alert, isVisible: false })
  });

  useEffect(() => {

    if (pathname) {

      const token = localStorage.getItem('token')

      let path = pathname.pathname
      if (path.includes("/page")) {
        path = path.split("/page")[0]
      }

      switch (path) {
        case "/login":
          if (token) navigate("/");
          break;
        case "/register":
          if (token) navigate("/");
          break;
        case "/forgotten-password":
          if (token) navigate("/");
          break;
        case "/account-config":
          if (!token) navigate("/login");
          break;
        case "/users":
          if (!token) navigate("/login");
          isAdmin()
          break;
        default:
          break;
      }
    }

  }, [pathname]);

  /* Checks if the logged in user is authorized to access a certain route,
    if not, it is redirected to the home page */
  async function isAdmin() {

    try {

      const response = await getDataAccount()
      const roleAdmin = response.data.roles.find(role => role.roleName == "ADMIN")

      if (!roleAdmin) navigate("/")

    } catch (error) {
      console.log(error);
      navigate("/")
    }

  }

  return (
    <>
      <Header />

      <Alert
        status={alert.status}
        isVisible={alert.isVisible}
        text={alert.text}
        closed={() => setAlert({ ...alert, isVisible: false })}
      />

      <MainContext.Provider value={{ setAlert }}>
        <main>
          <Outlet />
        </main>
      </MainContext.Provider>

      <footer className='flex flex-row items-center justify-between bg-white p-4 border-t-1'>
        
        <div className="flex flex-row">
          <a href="https://www.linkedin.com/in/jeandson-barros/" target="_blank"><BsLinkedin /></a>
          <a href="https://github.com/JeandsonBarros" target="_blank"><BsGithub /></a>
          <a href="https://www.instagram.com/jeandsonbarros/" target="_blank"><BsInstagram /></a>
        </div>

        <span>&copy; Jeandson Barros - 2023</span>

      </footer>
    </>
  )
}

export default App
