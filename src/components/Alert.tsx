import { Button } from '@nextui-org/react';
import { MdCheckCircle, MdInfo, MdOutlineClose, MdOutlineWarning, MdReport } from 'react-icons/md';
import styles from '../styles/components_styles/alert.module.css';

export interface AlertProps {
    text: string
    status?: string /* "info" | "error" | "warning" | "success" */
    isVisible: boolean
    closed: () => void
}

function Alert({ text, status, isVisible, closed }: AlertProps) {

    function iconStatus() {

        switch (status) {
            case 'info':
                return <MdInfo className={styles.info} />
            case 'success':
                return <MdCheckCircle className={styles.success} />
            case 'warning':
                return <MdOutlineWarning className={styles.warning} />
            case 'error':
                return <MdReport className={styles.error} />
            default:
                return <MdInfo className={styles.info} />
        }

    }

    return (
        <>
            {isVisible &&
                <div className={styles.alert}>

                    <div className="flex flex-row justify-between items-center">

                        <div className='flex flex-row items-center'>
                            {iconStatus()}
                            <h4>{status ? status.charAt(0).toUpperCase() + status.slice(1) : "Info"}</h4>
                        </div>

                        <Button
                            className="m-1"
                            onClick={closed}
                            isIconOnly
                            variant="light"
                            color="danger"
                            radius="full"
                        >
                            <MdOutlineClose />
                        </Button>

                    </div>

                    <p>{text}</p>

                </div>}
        </>
    );
}

export default Alert;