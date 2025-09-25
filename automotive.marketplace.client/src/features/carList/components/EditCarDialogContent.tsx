import { useSuspenseQuery } from "@tanstack/react-query";
import { getCarByIdOptions } from "../api/getCarByIdOptions";
import { CarFormData } from "../types/CarFormData";
import CarForm from "./CarForm";

type EditCarDialogContentProps = {
  id: string;
  onSubmit: (formData: CarFormData) => Promise<void>;
};

const EditCarDialogContent = ({ id, onSubmit }: EditCarDialogContentProps) => {
  const { data: carQuery } = useSuspenseQuery(getCarByIdOptions({ id }));
  const car = carQuery.data;
  return (
    <CarForm car={{ ...car, year: Number(car?.year) }} onSubmit={onSubmit} />
  );
};

export default EditCarDialogContent;
